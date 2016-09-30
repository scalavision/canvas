using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isas.SequencingFiles;
using Isas.Shared.DataTypes;
using Isas.Shared.Utilities.FileSystem;

namespace Canvas
{

    public class BaseCallset
    {
        public IDirectoryLocation OutputFolder { get; set; }
        public IDirectoryLocation WholeGenomeFastaFolder { get; set; }
        public IFileLocation KmerFasta { get; set; }
        public GenomeMetadata GenomeMetadata { get; set; }
        public IFileLocation FilterBed { get; set; }
        public IFileLocation PloidyBed { get; set; }

        public BaseCallset(IDirectoryLocation outputFolder, IDirectoryLocation wholeGenomeFastaFolder,
           IFileLocation kmerFasta, IFileLocation filterBed, IFileLocation ploidyBed)        
        {
            WholeGenomeFastaFolder = wholeGenomeFastaFolder;
            OutputFolder = outputFolder;
            KmerFasta = kmerFasta;
            FilterBed = filterBed;
            PloidyBed = ploidyBed;
        }
    }

    public class CanvasCallset : BaseCallset
    {
        public string Id => SampleName; // unique ID for this callset
        public string SampleName { get; }
        public Bam Bam { get; }
        public IEnumerable<Bam> NormalBamPaths { get; }
        public IFileLocation NormalVcfPath { get; } // set to the Starling VCF path (if tumor normal, the normal vcf path) 
        public bool IsDbSnpVcf { get; set; } // NormalVcfPath points to a dbSNP VCF file
        public IFileLocation SomaticVcfPath { get; } // set to the strelka VCF path
        public IFileLocation OutputVcfPath { get; }
        public NexteraManifest Manifest { get; }


        public CanvasCallset(
            IFileLocation bam,
            string sampleName,
            IDirectoryLocation wholeGenomeFastaFolder,
            IDirectoryLocation outputFolder,
            IFileLocation kmerFasta,
            IFileLocation filterBed,
            IFileLocation ploidyBed,
            IFileLocation normalVcfPath,
            bool isDbSnpVcf,
            IEnumerable<IFileLocation> normalBamPaths,
            NexteraManifest manifest,
            IFileLocation somaticVcfPath,
            IFileLocation outputVcfPath) : 
            base(outputFolder,
                wholeGenomeFastaFolder,
                kmerFasta,
                filterBed,
                ploidyBed)
        {
            Bam = new Bam(bam);
            SampleName = sampleName;
            WholeGenomeFastaFolder = wholeGenomeFastaFolder;
            OutputFolder = outputFolder;
            KmerFasta = kmerFasta;
            FilterBed = filterBed;
            PloidyBed = ploidyBed;
            NormalVcfPath = normalVcfPath;
            IsDbSnpVcf = isDbSnpVcf;
            Manifest = manifest;
            SomaticVcfPath = somaticVcfPath;
            OutputVcfPath = outputVcfPath;
            NormalBamPaths = normalBamPaths.Select(file => new Bam(file));
            var genomeSizeXml = WholeGenomeFastaFolder.GetFileLocation("GenomeSize.xml");
            GenomeMetadata = new GenomeMetadata();
            GenomeMetadata.Deserialize(genomeSizeXml.FullName);
        }

        public bool IsEnrichment => Manifest != null;

        internal string TempFolder
        {
            get { return Path.Combine(OutputFolder.FullName, String.Format("TempCNV_{0}", Id)); }
        }

        internal string NormalBinnedPath
        {
            get { return Path.Combine(TempFolder, String.Format("{0}.normal.binned", Id)); }
        }

        internal string BinSizePath
        {
            get { return Path.Combine(TempFolder, String.Format("{0}.binsize", Id)); }
        }

        internal string VfSummaryPath
        {
            get { return Path.Combine(TempFolder, String.Format("VFResults{0}.txt.gz", Id)); }
        }

        internal string VfSummaryBafPath
        {
            get { return VfSummaryPath + ".baf"; }
        }

        internal string TempManifestPath
        {
            get { return Path.Combine(TempFolder, "manifest.txt"); }
        }
    }

    public class SmallPedigreeCallset : BaseCallset
    {
        private IDirectoryLocation outputDirectory;
        private IEnumerable<IFileLocation> _bam;
        private List<string> list;
        private string sampleName;
        private IDirectoryLocation wholeGenomeFasta;
        private IEnumerable<IFileLocation> enumerable;

        public IDirectoryLocation OutputFolder { get; }
        public IEnumerable<string> SampleNames { get; }
        public IEnumerable<Bam> BamPaths { get; }
        public IEnumerable<IFileLocation> NormalVcfPaths { get; } // set to the Starling VCF path (if tumor normal, the normal vcf path) 
        public bool IsDbSnpVcf { get; set; } // NormalVcfPath points to a dbSNP VCF file
        public IFileLocation SomaticVcfPath { get; } // set to the strelka VCF path
        public IFileLocation OutputVcfPath { get; }
        public NexteraManifest Manifest { get; }


        public SmallPedigreeCallset(
            IDirectoryLocation outputFolder,
            IEnumerable<IFileLocation> bamPaths,
            IEnumerable<string> sampleNames,
            IDirectoryLocation wholeGenomeFastaFolder,
            IFileLocation kmerFasta,
            IFileLocation filterBed,
            IFileLocation ploidyBed,
            IEnumerable<IFileLocation> normalVcfPaths,
            IFileLocation outputVcfPath) :
            base(outputFolder,
                wholeGenomeFastaFolder,
                kmerFasta,
                filterBed,
                ploidyBed)
        {
            BamPaths = bamPaths.Select(file => new Bam(file));
            SampleNames = sampleNames;
            NormalVcfPaths = normalVcfPaths;
            WholeGenomeFastaFolder = wholeGenomeFastaFolder;
            OutputFolder = outputFolder;
            KmerFasta = kmerFasta;
            FilterBed = filterBed;
            PloidyBed = ploidyBed;
            OutputVcfPath = outputVcfPath;
            var genomeSizeXml = WholeGenomeFastaFolder.GetFileLocation("GenomeSize.xml");
            GenomeMetadata = new GenomeMetadata();
            GenomeMetadata.Deserialize(genomeSizeXml.FullName);
        }


        internal string TempFolder
        {
            get { return Path.Combine(OutputFolder.FullName, "TempCNV"); }
        }

        internal IEnumerable<string> NormalBinnedPath
        {
            get { return SampleNames.Select(sampleName => Path.Combine(TempFolder, $"{sampleName}.normal.binned")); }
        }

        internal IEnumerable<string> BinSizePath
        {
            get { return SampleNames.Select(sampleName => Path.Combine(TempFolder, $"{sampleName}.binsize")); }
        }

        internal IEnumerable<string> VfSummaryPath
        {
            get { return SampleNames.Select(sampleName => Path.Combine(TempFolder, $"VFResults{sampleName}.txt.gz")); }
        }
    }
}