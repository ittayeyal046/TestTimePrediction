using System;
using System.Collections.Generic;
using System.Linq;
using TTPService.Dtos.Creation;
using TTPService.Entities;
using TTPService.Enums;

namespace TTPService.Tests
{
    public class TestDataGenerationHelper
    {
        private static readonly Random _random = new Random();

        public static uint GenerateRandomUint()
        {
            return (uint)_random.Next(1 << 30) << 2 | (uint)_random.Next(1 << 2);
        }

        public static string GenerateUserName()
        {
            return $"RandomUser{_random.Next(1, 100)}";
        }

        public static ExperimentGroup GenerateExperimentGroupWithoutExperiments()
        {
            return new ExperimentGroup()
            {
                Id = Guid.NewGuid(),
                Username = GenerateUserName(),
                Experiments = new List<Experiment> { new Experiment() },
            };
        }

        public static IEnumerable<T> GenerateList<T>(Func<T> generationMethod, uint numberOfElements = 1)
        {
            var list = new List<T>();

            Enumerable.Range(1, checked((int)numberOfElements)).ToList().ForEach(i =>
                list.Add(generationMethod()));

            return list;
        }
    }
}
