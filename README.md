# Common .NET Libraries

A collection of reusable .NET libraries providing common functionality. These libraries are designed to reduce code duplication and provide standardized implementations for frequently used patterns and utilities.

## Overview

This repository contains multiple NuGet packages covering various aspects of .NET development:

- **Data Access**: Utilities for Dapper, FluentMigrator, SQL Server connections, and SqlKata query building
- **Extensions**: Common extension methods for base .NET types (DateTime, String, Span, etc.)
- **Messaging**: RabbitMQ integration utilities
- **File Handling**: AWS S3 compatible storage, compression, and file management utilities
- **Web Parsing**: HTML parsing and web scraping utilities
- **Testing**: Common test configuration and settings helpers

## Package Distribution

Packages are distributed through GitHub Packages as private NuGet packages under the `IMilosk.*` namespace.

## Contributing

This is a private repository for internal use. All packages follow semantic versioning and are automatically built and published through CI/CD pipelines.
