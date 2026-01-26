using MimeKit;
using MsgReader.Outlook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EmailConverge.Services
{
    public class EmailInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public DateTime? SentOn { get; set; }
        public string BodyText { get; set; } = string.Empty;
    }

    public class EmailParserService
    {
        public EmailInfo ParseMsgFile(string filePath)
        {
            var emailInfo = new EmailInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath)
            };

            try
            {
                using var msg = new Storage.Message(filePath);
                emailInfo.Subject = msg.Subject ?? string.Empty;
                emailInfo.From = msg.Sender?.Email ?? msg.Sender?.DisplayName ?? string.Empty;
                emailInfo.To = msg.GetEmailRecipients(MsgReader.Outlook.RecipientType.To, false, false) ?? string.Empty;
                emailInfo.SentOn = msg.SentOn;
                var rawBody = msg.BodyText ?? msg.BodyHtml ?? string.Empty;
                emailInfo.BodyText = CleanText(rawBody);
            }
            catch (Exception ex)
            {
                emailInfo.BodyText = $"解析错误: {ex.Message}";
            }

            return emailInfo;
        }

        public EmailInfo ParseEmlFile(string filePath)
        {
            var emailInfo = new EmailInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath)
            };

            try
            {
                var message = MimeMessage.Load(filePath);
                emailInfo.Subject = message.Subject ?? string.Empty;
                emailInfo.From = message.From?.ToString() ?? string.Empty;
                emailInfo.To = string.Join(", ", message.To.Select(t => t.ToString()));
                emailInfo.SentOn = message.Date.LocalDateTime;
                var rawBody = message.TextBody ?? message.HtmlBody ?? string.Empty;
                emailInfo.BodyText = CleanText(rawBody);
            }
            catch (Exception ex)
            {
                emailInfo.BodyText = $"解析错误: {ex.Message}";
            }

            return emailInfo;
        }

        public EmailInfo ParseEmailFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".eml" => ParseEmlFile(filePath),
                ".msg" => ParseMsgFile(filePath),
                _ => new EmailInfo
                {
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    BodyText = $"不支持的文件格式: {extension}"
                }
            };
        }

        public List<EmailInfo> ParseMultipleEmailFiles(IEnumerable<string> filePaths)
        {
            var results = new List<EmailInfo>();
            foreach (var filePath in filePaths)
            {
                results.Add(ParseEmailFile(filePath));
            }
            return results;
        }

        public List<EmailInfo> ParseMultipleMsgFiles(IEnumerable<string> filePaths)
        {
            var results = new List<EmailInfo>();
            foreach (var filePath in filePaths)
            {
                results.Add(ParseMsgFile(filePath));
            }
            return results;
        }

        public string GetCombinedText(IEnumerable<EmailInfo> emails)
        {
            var sb = new StringBuilder();
            foreach (var email in emails)
            {
                sb.AppendLine($"========== {email.FileName} ==========");
                sb.AppendLine($"主题: {email.Subject}");
                sb.AppendLine($"发件人: {email.From}");
                sb.AppendLine($"收件人: {email.To}");
                sb.AppendLine($"发送时间: {email.SentOn?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未知"}");
                sb.AppendLine();
                sb.AppendLine("正文内容:");
                sb.AppendLine(email.BodyText);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // 移除HTML标签
            text = Regex.Replace(text, @"<[^>]+>", " ");

            // 移除HTML实体
            text = Regex.Replace(text, @"&[a-zA-Z]+;", " ");
            text = Regex.Replace(text, @"&#\d+;", " ");

            // 移除URL
            text = Regex.Replace(text, @"https?://\S+", "");

            // 移除邮件签名分隔符后的内容（常见格式）
            text = Regex.Replace(text, @"(^|\n)[-_]{2,}\s*\n[\s\S]*$", "");

            // 移除多余的空白字符
            text = Regex.Replace(text, @"[\t\r]+", " ");
            text = Regex.Replace(text, @" {2,}", " ");
            text = Regex.Replace(text, @"\n{3,}", "\n\n");

            // 移除行首行尾空白
            text = Regex.Replace(text, @"^[ \t]+|[ \t]+$", "", RegexOptions.Multiline);

            // 移除常见的邮件引用标记
            text = Regex.Replace(text, @"^>+\s*", "", RegexOptions.Multiline);

            // 移除空行开头的邮件
            text = text.TrimStart();

            return text.Trim();
        }
    }
}
