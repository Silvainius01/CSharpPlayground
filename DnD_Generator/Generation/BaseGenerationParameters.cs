using System;
using System.Collections.Generic;
using System.Text;

namespace DnD_Generator
{
    abstract class BaseGenerationParameters
    {
        bool isValid = false;
        public List<QualityLevel> Qualities = new List<QualityLevel>();

        public BaseGenerationParameters(int numQualities, QualityLevel quality) { AddQuality(quality, numQualities); }
        public BaseGenerationParameters(int numQualities, Func<QualityLevel> DetermineQuality) { AddQuality(numQualities, DetermineQuality); }
        public BaseGenerationParameters(IEnumerable<QualityLevel> qualities) { AddQuality(qualities); }
        public BaseGenerationParameters(params QualityLevel[] qualities) { AddQuality(qualities); }

        public void AddQuality(QualityLevel q, int amount = 1)
        {
            for (int i = 0; i < amount; ++i)
                Qualities.Add(q);
        }
        public void AddQuality(IEnumerable<QualityLevel> qualities)
        {
            Qualities.AddRange(qualities);
        }
        public void AddQuality(int numQualities, Func<QualityLevel> DetermineQuality)
        {
            for (int i = 0; i < numQualities; ++i)
                Qualities.Add(DetermineQuality());
        }

        public void Validate()
        {
            if (isValid)
                return;
            isValid = ValidateInternal();
        }

        protected abstract bool ValidateInternal();
    }
}
