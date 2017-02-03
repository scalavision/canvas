﻿using System;
using System.Collections.Generic;
using System.IO;
using NDesk.Options;
using Newtonsoft.Json;
using System.Linq;
using Illumina.Common;
using Illumina.Common.FileSystem;
using Isas.Framework.Utilities;

namespace CanvasPedigreeCaller
{
    class Program
    {
        /// <summary>
        /// Command line help message.
        /// </summary>
        /// <param name="p">NDesk OptionSet</param>
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: CanvasPedigreeCaller.exe [OPTIONS]+");
            Console.WriteLine("Make discrete-valued copy number calls in a pedigree.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static int Main(string[] args)
        {
            CanvasCommon.Utilities.LogCommandLine(args);
            List<string> outFiles = new List<string>();
            List<string> segmentFiles = new List<string>();
            List<string> variantFrequencyFiles = new List<string>();
            string ploidyBedPath = null;
            string pedigreeFile = null;
            string referenceFolder = null;
            List<string> sampleNames = new List<string>();
            bool needHelp = false;
            string truthDataPath = null;

            string qualityScoreConfigPath = Path.Combine(Utilities.GetAssemblyFolder(typeof(Program)), "QualityScoreParameters.json");

            var p = new OptionSet()
            {
                { "i|infile=",        "file containing bins, their counts, and assigned segments (obtained from CanvasPartition.exe)",  v => segmentFiles.Add(v) },
                { "v|varfile=",       "file containing variant frequencies (obtained from CanvasSNV.exe)",                              v => variantFrequencyFiles.Add(v) },
                { "o|outfile=",       "name of output directory",                                                                       v => outFiles.Add(v) },
                { "r|reference=",     "reference genome folder that contains GenomeSize.xml",                                           v => referenceFolder = v },
                { "n|sampleName=",    "sample name for output VCF header (optional)",                                                   v => sampleNames.Add(v)},
                { "f|pedigree=",      "relationship withoin pedigree (parents/proband)",                                                v => pedigreeFile = v },
                { "p|ploidyBed=",     "bed file specifying reference ploidy (e.g. for sex chromosomes) (optional)",                     v => ploidyBedPath = v },
                { "h|help",           "show this message and exit",                                                                     v => needHelp = v != null },
                { "s|qscoreconfig=", $"parameter configuration path (default {qualityScoreConfigPath})",                                v => qualityScoreConfigPath = v },
                { "t|truth=", "path to vcf/bed with CNV truth data (optional)",                                                         v => truthDataPath = v },
            };

            List<string> extraArgs = p.Parse(args);

            if (extraArgs.Count > 0)
            {
                Console.WriteLine("* Error: I don't understand the argument '{0}'", extraArgs[0]);
                needHelp = true;
            }
            if (needHelp)
            {
                ShowHelp(p);
                return 0;
            }

            if (!segmentFiles.Any() || !outFiles.Any() || !variantFrequencyFiles.Any() || string.IsNullOrEmpty(referenceFolder))
            {
                ShowHelp(p);
                return 0;
            }

            foreach (string segmentFile in segmentFiles)
            {
                if (!File.Exists(segmentFile))
                {
                    Console.WriteLine($"CanvasPedigreeCaller.exe: File {segmentFile} does not exist! Exiting.");
                    return 1;
                }
            }

            foreach (string variantFrequencyFile in variantFrequencyFiles)
            {
                if (!File.Exists(variantFrequencyFile))
                {
                    Console.WriteLine($"CanvasPedigreeCaller.exe: File {variantFrequencyFile} does not exist! Exiting.");
                    return 1;
                }
            }


            if (!File.Exists(Path.Combine(referenceFolder, "GenomeSize.xml")))
            {
                Console.WriteLine($"CanvasPedigreeCaller.exe: File {Path.Combine(referenceFolder, "GenomeSize.xml")} does not exist! Exiting.");
                return 1;
            }

            // Set parameters:
            // caller.germlineScoreParameters = qscoreParametersJSON;
            // FileLocation qscoreConfigFile = new FileLocation(qualityScoreConfigPath);

            CanvasPedigreeCaller caller = new CanvasPedigreeCaller();
            if (pedigreeFile.IsNullOrEmpty())
            {

                Console.WriteLine($"CanvasPedigreeCaller.exe: pedigreeFile option is not used! Calling CNV variants without family information.");
                return caller.CallVariants(variantFrequencyFiles, segmentFiles, outFiles, ploidyBedPath, referenceFolder,
                    sampleNames, pedigreeFile);
            }
            if (!File.Exists(pedigreeFile))
            {
                Console.WriteLine($"CanvasPedigreeCaller.exe: File {pedigreeFile} does not exist! Exiting. Call variants without using family information.");
                return 1;
            }
            return caller.CallVariantsInPedigree(variantFrequencyFiles, segmentFiles, outFiles, ploidyBedPath, referenceFolder, sampleNames, pedigreeFile);
        }

        private static T Deserialize<T>(IFileLocation path)
        {
            using (StreamReader reader = path.OpenText())
                return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
        }
    }
}
