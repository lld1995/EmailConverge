namespace EmailConverge.Services
{
    public class TemplateItem
    {
        public SummaryTemplateType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public override string ToString() => Name;
    }

    public enum SummaryTemplateType
    {
        KeyPoints,
        Outline,
        ActionItems,
        Brief,
        Detailed,
        AnnualReview
    }

    public static class SummaryTemplates
    {
        public static string GetTemplateName(SummaryTemplateType type) => type switch
        {
            SummaryTemplateType.KeyPoints => "关键信息提取",
            SummaryTemplateType.Outline => "按提纲总结",
            SummaryTemplateType.ActionItems => "行动项提取",
            SummaryTemplateType.Brief => "简要摘要",
            SummaryTemplateType.Detailed => "详细分析",
            SummaryTemplateType.AnnualReview => "年度总结",
            _ => "关键信息提取"
        };

        public static string GetPrompt(SummaryTemplateType type, string emailContent) => type switch
        {
            SummaryTemplateType.KeyPoints => $"""
                请对以下邮件内容进行总结，提取关键信息：
                1. 主要议题/主题
                2. 关键人物和他们的观点
                3. 重要日期、数字或决定
                4. 需要跟进的行动项（如有）

                邮件内容：
                {emailContent}

                请用简洁的中文进行总结：
                """,

            SummaryTemplateType.Outline => $"""
                请按照以下提纲结构对邮件内容进行总结：

                一、背景与目的
                   - 邮件讨论的背景是什么
                   - 发送邮件的主要目的

                二、核心内容
                   - 主要讨论的议题
                   - 各方的观点和立场

                三、关键数据与事实
                   - 提到的重要数字、日期
                   - 关键的事实信息

                四、结论与决定
                   - 达成的共识或决定
                   - 未解决的问题

                五、后续行动
                   - 需要执行的任务
                   - 责任人和截止日期

                邮件内容：
                {emailContent}

                请按上述提纲格式输出总结：
                """,

            SummaryTemplateType.ActionItems => $"""
                请从以下邮件中提取所有行动项和待办事项：

                对于每个行动项，请列出：
                - 任务描述
                - 责任人（如有提及）
                - 截止日期（如有提及）
                - 优先级（根据上下文判断：高/中/低）

                邮件内容：
                {emailContent}

                请以清单形式列出所有行动项：
                """,

            SummaryTemplateType.Brief => $"""
                请用2-3句话简要概括以下邮件的核心内容，突出最重要的信息：

                邮件内容：
                {emailContent}

                简要摘要：
                """,

            SummaryTemplateType.Detailed => $"""
                请对以下邮件进行详细分析：

                1. 邮件概述
                   - 发件人意图
                   - 邮件类型（通知/请求/讨论/汇报等）

                2. 详细内容分析
                   - 逐段分析邮件内容
                   - 提取每段的关键信息

                3. 涉及人员
                   - 列出所有提到的人员
                   - 说明他们的角色和观点

                4. 时间线
                   - 按时间顺序列出所有提到的日期和事件

                5. 数据与指标
                   - 列出所有提到的数字和指标

                6. 问题与风险
                   - 识别邮件中提到的问题
                   - 潜在的风险点

                7. 建议与行动
                   - 提出的建议
                   - 需要采取的行动

                邮件内容：
                {emailContent}

                详细分析报告：
                """,

            SummaryTemplateType.AnnualReview => $"""
                以下是一段时间内的日报/周报内容，请基于这些内容生成一份年度总结报告：

                一、年度工作概述
                   - 主要工作方向和职责范围
                   - 整体工作节奏和重点

                二、核心成果与业绩
                   - 完成的重大项目/任务
                   - 取得的关键成果和突破
                   - 量化的业绩指标（如有）

                三、工时及占比统计
                   - 各项目/任务投入的总工时计算
                   - 不同工作类型的时间占比（如开发、会议、沟通、学习等）
                   - 主要精力分配情况分析

                四、成长与协作
                   - 学习和掌握的新技能、专业能力提升
                   - 跨部门协作与团队贡献

                五、反思与展望
                   - 遇到的主要困难与解决方法
                   - 下年工作计划与改进方向

                日报/周报内容：
                {emailContent}

                请生成结构化的年度总结报告：
                """,

            _ => $"""
                请对以下邮件内容进行总结：

                邮件内容：
                {emailContent}

                总结：
                """
        };

        public static SummaryTemplateType[] GetAllTypes() => new[]
        {
            SummaryTemplateType.KeyPoints,
            SummaryTemplateType.Outline,
            SummaryTemplateType.ActionItems,
            SummaryTemplateType.Brief,
            SummaryTemplateType.Detailed,
            SummaryTemplateType.AnnualReview
        };
    }
}
