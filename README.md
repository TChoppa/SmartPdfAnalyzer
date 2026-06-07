# PdfRag

A powerful PDF processing and Retrieval-Augmented Generation (RAG) system built with C# and modern web technologies.

## Overview

PdfRag is a comprehensive solution for extracting, processing, and intelligently retrieving information from PDF documents. By combining robust backend processing with an intuitive web interface, PdfRag enables seamless document analysis and retrieval-augmented generation capabilities.

## Features

- 📄 **PDF Processing** - Efficient PDF parsing and text extraction
- 🔍 **Intelligent Retrieval** - RAG-powered document search and retrieval
- 🌐 **Web Interface** - User-friendly HTML-based UI for document management
- 🔐 **Secure Processing** - Enterprise-grade security for sensitive documents
- ⚡ **Fast Performance** - Optimized C# backend for rapid processing
- 📊 **Advanced Analytics** - Document statistics and metadata extraction

## Technology Stack

- **Backend**: C# (.NET)
- **Frontend**: HTML5, CSS, JavaScript
- **Language Composition**: 
  - HTML: 64.8%
  - C#: 35.2%

## Getting Started

### Prerequisites

- .NET Runtime or SDK (version 6.0 or higher)
- Modern web browser
- 2GB RAM minimum

### Installation

1. Clone the repository:
```bash
git clone https://github.com/TChoppa/PdfRag.git
cd PdfRag
```

2. Build the project:
```bash
dotnet build
```

3. Run the application:
```bash
dotnet run
```

4. Open your browser and navigate to:
```
http://localhost:5000
```

## Usage

### Basic PDF Upload
1. Navigate to the upload section
2. Select your PDF file
3. Click "Process"
4. View extracted content and metadata

### Document Retrieval
1. Search for specific content within processed documents
2. Use advanced filters for precise retrieval
3. Export results in multiple formats

### RAG Integration
- Query documents using natural language
- Retrieve contextually relevant information
- Generate summaries and insights

## Project Structure

```
PdfRag/
├── src/
│   ├── Backend/           # C# backend services
│   ├── Frontend/          # HTML/CSS/JS web interface
│   └── Core/              # Core PDF processing logic
├── README.md              # This file
├── LICENSE                # License information
└── docs/                  # Additional documentation
```

## Configuration

Configuration settings can be modified in `appsettings.json`:

```json
{
  "PdfProcessing": {
    "MaxFileSize": 50,
    "SupportedFormats": ["pdf"],
    "ExtractMetadata": true
  }
}
```

## Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is available under the MIT License. See the LICENSE file for more details.

## Support & Feedback

- 📧 **Email**: Open an issue on GitHub
- 🐛 **Bug Reports**: Use the Issues tab
- 💡 **Feature Requests**: Discussions tab

## Roadmap

- [ ] Advanced OCR integration
- [ ] Multi-language support
- [ ] Cloud storage integration
- [ ] Real-time collaboration features
- [ ] API documentation
- [ ] Performance optimizations

## Acknowledgments

- Thanks to all contributors and users
- Built with modern web and .NET technologies
- Community feedback drives our development

## Author

**TChoppa** - [GitHub Profile](https://github.com/TChoppa)

---

**Last Updated**: June 2026

For more information, visit the [GitHub repository](https://github.com/TChoppa/PdfRag)
