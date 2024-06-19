using FeedbackExtractor.OpenAI.Entities;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackExtractor.OpenAI.Extensions
{
    internal static class ImageDetailParameterExtensions
    {
        public static ImageChatMessageContentPartDetail ToImageChatMessageDetail(this ImageDetailParameter imageDetail)
        {
            switch (imageDetail)
            {
                case ImageDetailParameter.Auto:
                default:
                    return ImageChatMessageContentPartDetail.Auto;
                case ImageDetailParameter.Low:
                    return ImageChatMessageContentPartDetail.Low;
                case ImageDetailParameter.High:
                    return ImageChatMessageContentPartDetail.High;
            }
        }
    }
}
