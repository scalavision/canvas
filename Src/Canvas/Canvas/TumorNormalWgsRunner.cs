﻿using System.Linq;
using Canvas.CommandLineParsing;
using CanvasCommon;
using Illumina.SecondaryAnalysis;
using Isas.Shared.Checkpointing;
using Isas.Shared.Utilities;
using Isas.Shared.Utilities.FileSystem;

namespace Canvas
{
    public class TumorNormalWgsRunner : IModeRunner
    {
        private readonly TumorNormalOptions _tumorNormalOptions;
        public CommonOptions CommonOptions { get; }
        public SingleSampleCommonOptions SingleSampleCommonOptions { get; }

        public TumorNormalWgsRunner(CommonOptions commonOptions, SingleSampleCommonOptions singleSampleCommonOptions, TumorNormalOptions tumorNormalOptions)
        {
            _tumorNormalOptions = tumorNormalOptions;
            CommonOptions = commonOptions;
            SingleSampleCommonOptions = singleSampleCommonOptions;
        }

        public void Run(ILogger logger, ICheckpointRunnerAsync checkpointRunner, IWorkManager workManager)
        {
            CanvasRunner runner = new CanvasRunner(logger, workManager, checkpointRunner, true, CanvasCoverageMode.GCContentWeighted, 100, CommonOptions.CustomParams);
            var callset = GetCallset();
            runner.CallSample(callset);
        }

        private CanvasCallset GetCallset()
        {
            IFileLocation outputVcfPath = CommonOptions.OutputDirectory.GetFileLocation("CNV.vcf.gz");
            CanvasCallset callSet = new CanvasCallset(
                    _tumorNormalOptions.TumorBam,
                    SingleSampleCommonOptions.SampleName,
                    CommonOptions.WholeGenomeFasta,
                    CommonOptions.OutputDirectory,
                    CommonOptions.KmerFasta,
                    CommonOptions.FilterBed,
                    SingleSampleCommonOptions.PloidyBed,
                    SingleSampleCommonOptions.BAlleleSites,
                    SingleSampleCommonOptions.IsDbSnpVcf,
                    Enumerable.Empty<IFileLocation>(),
                    null,
                    _tumorNormalOptions.SomaticVcf,
                    outputVcfPath);
            return callSet;
        }
    }
}