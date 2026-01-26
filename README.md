<p align="center">
  <img src="https://img.icons8.com/fluency/96/email-open.png" alt="EmailConverge Logo" width="96" height="96">
</p>

<h1 align="center">📧 EmailConverge</h1>

<p align="center">
  <strong>邮件智能总结助手 - 基于 AI 的邮件内容分析与摘要工具</strong>
</p>

<p align="center">
  <a href="#功能特性">功能特性</a> •
  <a href="#快速开始">快速开始</a> •
  <a href="#使用指南">使用指南</a> •
  <a href="#配置说明">配置说明</a> •
  <a href="#技术栈">技术栈</a> •
  <a href="#许可证">许可证</a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet" alt=".NET 9.0">
  <img src="https://img.shields.io/badge/Avalonia-11.3-8B5CF6?style=flat-square" alt="Avalonia UI">
  <img src="https://img.shields.io/badge/License-MIT-green?style=flat-square" alt="MIT License">
  <img src="https://img.shields.io/badge/Platform-Windows-0078D6?style=flat-square&logo=windows" alt="Windows">
</p>

---

## ✨ 功能特性

- **📂 多格式支持** - 支持解析 `.msg` (Outlook) 和 `.eml` 标准邮件格式
- **🤖 AI 智能总结** - 集成 OpenAI 兼容 API，支持本地模型 (Ollama) 和云端服务
- **📝 多种总结模板** - 提供 6 种专业总结模板，满足不同场景需求
- **⚡ 流式输出** - 实时显示 AI 生成内容，提升用户体验
- **📊 大文件处理** - 自动分段处理超长邮件内容，智能合并总结
- **🎨 现代化界面** - 基于 Avalonia UI 的 Fluent 设计风格

## 📋 总结模板

| 模板 | 说明 |
|------|------|
| 🔑 **关键信息提取** | 提取主题、关键人物、重要日期和行动项 |
| 📑 **按提纲总结** | 结构化输出：背景、核心内容、结论、后续行动 |
| ✅ **行动项提取** | 提取任务、责任人、截止日期和优先级 |
| 📌 **简要摘要** | 2-3 句话概括核心内容 |
| 📊 **详细分析** | 全面分析邮件的各个维度 |
| 📅 **年度总结** | 基于日报/周报生成年度工作总结 |

## 🚀 快速开始

### 系统要求

- Windows 10/11
- .NET 9.0 Runtime

### 安装方式

#### 方式一：下载安装包

下载 [EmailConverge_Setup.exe](./EmailConverge_Setup.exe) 并运行安装程序。

#### 方式二：从源码构建

```bash
# 克隆仓库
git clone https://github.com/your-username/EmailConverge.git
cd EmailConverge

# 构建项目
dotnet build -c Release

# 运行应用
dotnet run --project EmailConverge
```

## 📖 使用指南

### 1. 选择邮件文件

点击左侧面板的 **「选择邮件文件」** 按钮，支持批量选择 `.msg` 或 `.eml` 文件。

### 2. 查看解析内容

选择文件后，中间面板将自动显示解析后的邮件内容，包括：
- 邮件主题
- 发件人 / 收件人
- 发送时间
- 正文内容

### 3. 配置 AI 模型

点击右侧面板的 **「模型配置」** 按钮，设置：

| 配置项 | 说明 | 示例 |
|--------|------|------|
| API 地址 | OpenAI 兼容的 API 端点 | `http://localhost:11434/v1` |
| API Key | API 密钥（本地模型可填 `ollama`） | `sk-xxx` 或 `ollama` |
| 模型名称 | 使用的模型 | `qwen2.5:7b`、`gpt-4o` |

### 4. 生成 AI 总结

1. 选择合适的 **总结模板**
2. 点击 **「开始 AI 总结」** 按钮
3. 等待 AI 流式输出总结结果

## ⚙️ 配置说明

### 本地模型 (Ollama)

```
API 地址: http://localhost:11434/v1
API Key: ollama
模型名称: qwen2.5:7b (或其他已安装的模型)
```

### OpenAI

```
API 地址: https://api.openai.com/v1
API Key: sk-your-api-key
模型名称: gpt-4o / gpt-4o-mini
```

### 其他兼容服务

支持任何 OpenAI API 兼容的服务，如：
- Azure OpenAI
- Claude (通过兼容层)
- 国内大模型 API

## 🛠️ 技术栈

| 组件 | 技术 |
|------|------|
| **框架** | .NET 9.0 |
| **UI** | Avalonia UI 11.3 (Fluent Theme) |
| **邮件解析** | MsgReader 5.5、MimeKit 4.3 |
| **AI 集成** | Microsoft.Extensions.AI.OpenAI |

## 📁 项目结构

```
EmailConverge/
├── EmailConverge/
│   ├── Services/
│   │   ├── AiConfig.cs           # AI 配置管理
│   │   ├── AiSummaryService.cs   # AI 总结服务
│   │   ├── EmailParserService.cs # 邮件解析服务
│   │   └── SummaryTemplates.cs   # 总结模板定义
│   ├── App.axaml                 # 应用程序入口
│   ├── MainWindow.axaml          # 主窗口界面
│   └── EmailConverge.csproj       # 项目文件
├── installer.nsi                 # NSIS 安装脚本
├── LICENSE.txt                   # MIT 许可证
└── README.md                     # 项目说明
```

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 提交 Pull Request

## 📄 许可证

本项目基于 [MIT 许可证](LICENSE.txt) 开源。

---

<p align="center">
  Made with ❤️ by EmailConverge Contributors
</p>
