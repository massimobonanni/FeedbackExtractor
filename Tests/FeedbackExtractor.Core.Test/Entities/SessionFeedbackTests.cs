using FeedbackExtractor.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackExtractor.Core.Test.Entities
{
    public class SessionFeedbackTests
    {
        #region IsValid
        [Fact]
        public void IsValid_WhenEventNameIsNull_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = null,
                SessionCode = "123",
                EventQuality = 5,
                SessionQuality = 5,
                SpeakerQuality = 5
            };

            Assert.False(feedback.IsValid());
        }

        [Fact]
        public void IsValid_WhenEventNameIsEmpty_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = string.Empty,
                SessionCode = "123",
                EventQuality = 5,
                SessionQuality = 5,
                SpeakerQuality = 5
            };

            Assert.False(feedback.IsValid());
        }
        [Fact]
        public void IsValid_WhenEventNameIsWhitespace_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = "   ",
                SessionCode = "123",
                EventQuality = 5,
                SessionQuality = 5,
                SpeakerQuality = 5
            };

            Assert.False(feedback.IsValid());
        }

        [Fact]
        public void IsValid_WhenSessionCodeIsNull_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = "Event",
                SessionCode = null,
                EventQuality = 5,
                SessionQuality = 5,
                SpeakerQuality = 5
            };

            Assert.False(feedback.IsValid());
        }

        [Fact]
        public void IsValid_WhenSessionCodeIsEmpty_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = "Event",
                SessionCode = string.Empty,
                EventQuality = 5,
                SessionQuality = 5,
                SpeakerQuality = 5
            };

            Assert.False(feedback.IsValid());
        }

        [Fact]
        public void IsValid_WhenSessionCodeIsWhitespace_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = "Event",
                SessionCode = "  ",
                EventQuality = 5,
                SessionQuality = 5,
                SpeakerQuality = 5
            };

            Assert.False(feedback.IsValid());
        }

        [Fact]
        public void IsValid_WhenEventQualityIsNull_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = "Event",
                SessionCode = "123",
                EventQuality = null,
                SessionQuality = 5,
                SpeakerQuality = 5
            };

            Assert.False(feedback.IsValid());
        }

        [Fact]
        public void IsValid_WhenEventQualityIsLessThanOne_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = "Event",
                SessionCode = "123",
                EventQuality = 0,
                SessionQuality = 5,
                SpeakerQuality = 5
            };

            Assert.False(feedback.IsValid());
        }

        [Fact]
        public void IsValid_WhenEventQualityIsGreaterThanFive_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = "Event",
                SessionCode = "123",
                EventQuality = 6,
                SessionQuality = 5,
                SpeakerQuality = 5
            };

            Assert.False(feedback.IsValid());
        }

        [Fact]
        public void IsValid_WhenSessionQualityIsNull_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = "Event",
                SessionCode = "123",
                EventQuality = 5,
                SessionQuality = null,
                SpeakerQuality = 5
            };

            Assert.False(feedback.IsValid());
        }

        [Fact]
        public void IsValid_WhenSessionQualityIsLessThanOne_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = "Event",
                SessionCode = "123",
                EventQuality = 5,
                SessionQuality = 0,
                SpeakerQuality = 5
            };

            Assert.False(feedback.IsValid());
        }

        [Fact]
        public void IsValid_WhenSessionQualityIsGreaterThanFive_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = "Event",
                SessionCode = "123",
                EventQuality = 5,
                SessionQuality = 6,
                SpeakerQuality = 5
            };

            Assert.False(feedback.IsValid());
        }

        [Fact]
        public void IsValid_WhenSpeakerQualityIsNull_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = "Event",
                SessionCode = "123",
                EventQuality = 5,
                SessionQuality = 5,
                SpeakerQuality = null
            };

            Assert.False(feedback.IsValid());
        }

        [Fact]
        public void IsValid_WhenSpeakerQualityIsLessThanOne_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = "Event",
                SessionCode = "123",
                EventQuality = 5,
                SessionQuality = 5,
                SpeakerQuality = 0
            };

            Assert.False(feedback.IsValid());
        }

        [Fact]
        public void IsValid_WhenSpeakerQualityIsGreaterThanFive_ReturnsFalse()
        {
            var feedback = new SessionFeedback
            {
                EventName = "Event",
                SessionCode = "123",
                EventQuality = 5,
                SessionQuality = 5,
                SpeakerQuality = 6
            };

            Assert.False(feedback.IsValid());
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetValidSessionFeedbacks), MemberType = typeof(DataGenerator))]
        public void IsValid_WhenAllPropertiesAreValid_ReturnsTrue(SessionFeedback feedback)
        {
            Assert.True(feedback.IsValid());
        }

        private class DataGenerator
        {
            public static IEnumerable<object[]> GetValidSessionFeedbacks()
            {
                for (int eventQuality = 1; eventQuality <= 5; eventQuality++)
                {
                    for (int sessionQuality = 1; sessionQuality <= 5; sessionQuality++)
                    {
                        for (int speakerQuality = 1; speakerQuality <= 5; speakerQuality++)
                        {
                            yield return  new object[]{ new SessionFeedback
                                {
                                    EventName = "Event",
                                    SessionCode = "123",
                                    EventQuality = eventQuality,
                                    SessionQuality = sessionQuality,
                                    SpeakerQuality = speakerQuality
                                }
                            };
                        }
                    }
                }
            }
        }
        #endregion Private Classes
    }
}
