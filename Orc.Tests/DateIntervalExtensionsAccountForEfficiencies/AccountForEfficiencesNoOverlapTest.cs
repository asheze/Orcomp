﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Orc.Entities;
using Orc.Extensions;

namespace Orx.Tests
{
    [TestFixture]
    public class AccountForEfficienciesNoOverlapTest
    {
        DateTime now;
        DateTime start;
        List<DateIntervalEfficiency> dateIntervalEfficiencies;

        [SetUp]
        public void Init()
        {
            this.now = DateTime.Now;
            this.start = now;
            this.dateIntervalEfficiencies = new List<DateIntervalEfficiency>();
        }

        [TearDown]
        public void Cleanup()
        {
            this.dateIntervalEfficiencies = null;
        }

        [Test]
        public void AccountForEfficiencies_NoOverlap_OneInterval()
        {
            // +--------------------------------+      Duration: 60 mins
            // |--------|                              Duration: 30 mins at 20%
            //
            // Result
            // |----------------|                      Duration: 75 mins


            // Arrange
            DateTime end = start.AddMinutes(60);

            var dateInterval = new DateInterval(start, end);

            var efficiency1 = new DateIntervalEfficiency(new DateInterval(start, start.AddMinutes(30)), 20); // 30 mins at 20%
            dateIntervalEfficiencies.Add(efficiency1);

            // Act
            var newDateInterval = dateInterval.AccountForEfficiencies(dateIntervalEfficiencies);

            // Assert
            var result = new DateInterval(start, start.AddMinutes(75));  // Duration: 75 mins
            Assert.AreEqual(result, newDateInterval);
        }

        [Test]
        public void AccountForEfficiencies_NoOverlap_TwoIntervals()
        {
            // +------------------+      Duration: 60 mins
            // |--------|                Duration: 30 mins at 20%
            //          |---------|      Duration: 30 mins at 150%
            //
            // Result 
            // 30 mins at 20%  => 6 mins at 100%
            // 30 mins at 150% => 45 mins at 100%
            // 60 - 6 - 45 = 9 mins
            //
            // Total = 30 + 30 + 9 = 69 mins

            // Arrange
            DateTime end = start.AddMinutes(60);

            var dateInterval = new DateInterval(start, end);

            var efficiency1 = new DateIntervalEfficiency(new DateInterval(start, start.AddMinutes(30)), 20); // 30 mins at 20%
            var efficiency2 = new DateIntervalEfficiency(new DateInterval(start.AddMinutes(30), start.AddMinutes(60)), 150); // 30 mins at 20%
            dateIntervalEfficiencies.Add(efficiency1);
            dateIntervalEfficiencies.Add(efficiency2);

            // Act
            var newDateInterval = dateInterval.AccountForEfficiencies(dateIntervalEfficiencies);

            // Assert
            var result = new DateInterval(start, start.AddMinutes(69));  // Duration: 69 mins
            Assert.AreEqual(result, newDateInterval);
        }

        [Test]
        public void AccountForEfficiencies_NoOverlap_TwoIntervals2()
        {
            // +------------------+     Duration: 60 mins
            //           |--------|     Duration: 20 mins at 40%
            //     |-----|              Duration: 10 mins at 150%
            //
            // Result 
            // 20 mins at 40%  => 8 mins at 100%
            // 10 mins at 150% => 1.5 mins at 100%
            // 60 - 8 - 1.5 = 50.5
            //
            // Total = 20 + 10 + 50.5 = 80.5 mins


            // Arrange
            DateTime end = start.AddMinutes(60);

            var dateInterval = new DateInterval(start, end);

            var efficiency1 = new DateIntervalEfficiency(new DateInterval(start.AddMinutes(30), start.AddMinutes(50)), 40); 
            var efficiency2 = new DateIntervalEfficiency(new DateInterval(start.AddMinutes(50), start.AddMinutes(60)), 150); 
            dateIntervalEfficiencies.Add(efficiency1);
            dateIntervalEfficiencies.Add(efficiency2);

            // Act
            var newDateInterval = dateInterval.AccountForEfficiencies(dateIntervalEfficiencies);

            // Assert
            var result = new DateInterval(start, start.AddMinutes(80.5));
            Assert.AreEqual(result, newDateInterval);
        }

        [Test]
        public void AccountForEfficiencies_NoOverlap_ThreeIntervals()
        {
            // +------------------+                         Duration: 70 mins
            // |------------------|                         Duration: 60 mins at 10%
            //                    |---------|               Duration: 40 mins at 80%
            //                              |---------|     Duration: 80 mins at 200%
            //
            // Result 
            // 60 mins at 10%  => 6 mins at 100%
            // 40 mins at 80%  => 32 mins at 100%
            // 80 mins at 200% => 160 mins at 100%
            //
            // 70 - 6 - 32 = 32 mins
            // 32 mins at 100% => 16 mins at 200%
            //
            // Total = 60 + 40 + 16 = 116 mins


            // Arrange
            DateTime end = start.AddMinutes(70);

            var dateInterval = new DateInterval(start, end);

            var efficiency1 = new DateIntervalEfficiency(new DateInterval(start, start.AddMinutes(60)), 10);
            var efficiency2 = new DateIntervalEfficiency(new DateInterval(start.AddMinutes(60), start.AddMinutes(100)), 80);
            var efficiency3 = new DateIntervalEfficiency(new DateInterval(start.AddMinutes(100), start.AddMinutes(180)), 200);
            dateIntervalEfficiencies.Add(efficiency1);
            dateIntervalEfficiencies.Add(efficiency2);
            dateIntervalEfficiencies.Add(efficiency3);

            // Act
            var newDateInterval = dateInterval.AccountForEfficiencies(dateIntervalEfficiencies);

            // Assert
            var result = new DateInterval(start, start.AddMinutes(116));
            Assert.AreEqual(result, newDateInterval);
        }

        [Test]
        public void AccountForEfficiencies_NoOverlap_ThreeIntervals2()
        {
            // +----------------------+                      Duration: 80 mins
            // |-------|                                     Duration: 20 mins at 10%
            //         |-------------|                       Duration: 50 mins at 60%
            //                       |------------|          Duration: 40 mins at 150%

            // Details:

            // 20 mins at 10%  => 2 min at 100%
            // 50 mins at 60%  => 30 min at 100%
            // 40 mins at 150% => 60 min at 100%
            //
            // 80 - 2 - 30 = 48min
            // 48 mins at 100% => 32 mins at 150%
            //
            // Final result: 20 + 50 + 32 = 102 mins 

            // Arrange
            DateTime end = start.AddMinutes(80);

            var dateInterval = new DateInterval(start, end);

            var efficiency1 = new DateIntervalEfficiency(new DateInterval(start, start.AddMinutes(20)), 10);
            var efficiency2 = new DateIntervalEfficiency(new DateInterval(start.AddMinutes(20), start.AddMinutes(70)), 60);
            var efficiency3 = new DateIntervalEfficiency(new DateInterval(start.AddMinutes(70), start.AddMinutes(110)), 150);
            dateIntervalEfficiencies.Add(efficiency1);
            dateIntervalEfficiencies.Add(efficiency2);
            dateIntervalEfficiencies.Add(efficiency3);

            // Act
            var newDateInterval = dateInterval.AccountForEfficiencies(dateIntervalEfficiencies);

            // Assert
            var result = new DateInterval(start, start.AddMinutes(102));  // Duration: 102 mins
            Assert.AreEqual(result, newDateInterval);
        }

        [Test]
        public void AccountForEfficiencies_NoOverlap_ThreeIntervals3()
        {
            //         +----------------------+     Duration: 60 mins
            // |-------|                            Duration: 30 mins at 30% (outside)
            //         |-------|                    Duration: 25 mins at 200%
            //                 |---|                Duration: 15 mins at 50%

            // Details:
            // 30 mins at 30%   => 0 (outside)
            // 25 mins at 200%  => 50 min at 100%
            // 15 mins at 50%   => 7.5 min at 100%
            // 60 - 50 - 7.5 = 2.5 min  
            //
            // Result: 25 + 15 + 2.5 = 42.5 mins.

            // Arrange
            DateTime end = start.AddMinutes(60);

            var dateInterval = new DateInterval(start, end);

            var efficiency1 = new DateIntervalEfficiency(new DateInterval(start.AddMinutes(-30), start), 30);
            var efficiency2 = new DateIntervalEfficiency(new DateInterval(start, start.AddMinutes(25)), 200);
            var efficiency3 = new DateIntervalEfficiency(new DateInterval(start.AddMinutes(25), start.AddMinutes(40)), 50);
            dateIntervalEfficiencies.Add(efficiency1);
            dateIntervalEfficiencies.Add(efficiency2);
            dateIntervalEfficiencies.Add(efficiency3);

            // Act
            var newDateInterval = dateInterval.AccountForEfficiencies(dateIntervalEfficiencies);

            // Assert
            var result = new DateInterval(start, start.AddMinutes(42.5));  // Duration: 42.5 mins
            Assert.AreEqual(result, newDateInterval);
        }

        [Test]
        public void AccountForEfficiencies_FixedEnd_NoOverlap_ThreeIntervals4()
        {
            // NOTE: This test if with Max end point fixed
            //
            // +----------------------+         Duration: 70 mins
            // |----------|                     Duration: 30 mins at 30%
            //            |-----|               Duration: 15 mins at 200%
            //                  |-|             Duration: 5 mins at 50% 

            // The first section is not affected by a calendar period (i.e. 70 - 30 - 15 - 5 = 20)
            // So only 70 - 20 = 50 mins are affected by calendar periods.
            // Details:
            // 30 mins at 30%   => 9 min at 100%
            // 15 mins at 200%  => 30 min at 100%
            // 5 mins at 50%    => 2.5 min at 100%
            // 50 - 9 - 30 - 2.5 = 8.5 mins

            // So the 50 mins affected by the calendar period will last 30 + 15 + 5 + 8.5 = 58.5 mins
            // Total duration 25 + 58.5 = 83.5 mins

            // Arrange
            DateTime end = start.AddMinutes(70);

            var dateInterval = new DateInterval(start, end);

            var efficiency1 = new DateIntervalEfficiency(new DateInterval(start, start.AddMinutes(30)), 30);
            var efficiency2 = new DateIntervalEfficiency(new DateInterval(start.AddMinutes(30), start.AddMinutes(45)), 200);
            var efficiency3 = new DateIntervalEfficiency(new DateInterval(start.AddMinutes(45), start.AddMinutes(50)), 50);
            dateIntervalEfficiencies.Add(efficiency1);
            dateIntervalEfficiencies.Add(efficiency2);
            dateIntervalEfficiencies.Add(efficiency3);

            // Act
            var newDateInterval = dateInterval.AccountForEfficiencies(dateIntervalEfficiencies, FixedEndPoint.Max);

            // Assert
            var result = new DateInterval(end.AddMinutes(-83.5), end); 
            Assert.AreEqual(result, newDateInterval);
        }


    }
}