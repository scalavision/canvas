﻿
using System.Collections.Generic;
using System.Linq;
using Illumina.Common;

namespace CanvasCommon
{
    public class CanvasFilter
    {

        public const int SegmentSizeCutoff = 10000;
        public const string Pass = "PASS";
        public const string AllSampleFailedTag = "FailedFT";
        public static readonly CanvasFilter PassFilter = Create(Enumerable.Empty<string>());
        private IReadOnlyList<string> FailedFilterTags { get; }
        public bool IsPass => FailedFilterTags.Count == 0;

        private CanvasFilter(IReadOnlyList<string> failedFilterTags)
        {
            FailedFilterTags = failedFilterTags;
        }

        public static CanvasFilter Create(IEnumerable<string> filterTags)
        {
            var allFailedFilterTags = filterTags.Where(failedFilterTag => failedFilterTag != Pass).ToList();
            return new CanvasFilter(allFailedFilterTags.ToReadOnlyList());
        }

        public CanvasFilter AddFilter(string newTag)
        {
            List<string> allTags = this.FailedFilterTags.ToList();
            allTags.Add(newTag);
            return CanvasFilter.Create(allTags);
        }

        public static CanvasFilter UpdateRecordLevelFilter(CanvasFilter recordLevelFilter,
            IReadOnlyList<CanvasFilter> sampleFilters) => sampleFilters.Any(x => x.IsPass)
                ? recordLevelFilter
                : Create(recordLevelFilter.FailedFilterTags.Concat(AllSampleFailedTag));

        public static CanvasFilter GetRecordLevelFilterFromSampleFiltersOnly(IReadOnlyList<CanvasFilter> sampleFilters) => UpdateRecordLevelFilter(PassFilter, sampleFilters);


        public string ToVcfString() => IsPass ? Pass : string.Join(";", FailedFilterTags);


        public static string FormatCnvSizeWithSuffix(int size)
        {
            if (size >= 1000000)
                return FormatCnvSizeWithSuffix(size / 1000000) + "MB";
            if (size >= 1000)
                return FormatCnvSizeWithSuffix(size / 1000) + "KB";
            return size.ToString();
        }
    }
}
