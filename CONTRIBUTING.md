# Contributing to AutomaticAds CS2

Thank you for your interest in contributing to AutomaticAds CS2! This document provides guidelines and information for contributors.

## Ways to Contribute

### 🐛 Bug Reports
- Use the [GitHub Issues](https://github.com/wiruwiru/AutomaticAds-CS2/issues) page
- **Tag your issue with `[BUG]`** in the title
- Include detailed steps to reproduce the issue
- Provide server information (CS2 version, plugin version, etc.)
- Include relevant configuration snippets if applicable

### 💡 Feature Requests
- Submit feature requests through [GitHub Issues](https://github.com/wiruwiru/AutomaticAds-CS2/issues)
- **Tag your issue with `[REQ]` or `[SUG]`** in the title
- Clearly describe the proposed feature and its use case
- Explain how it would benefit the community

### 📝 Documentation
- Help improve documentation clarity and accuracy
- Add examples for complex configurations
- Translate documentation to other languages
- Fix typos and grammatical errors

### 🔧 Code Contributions
- Bug fixes and performance improvements
- New features and enhancements
- Code optimization and refactoring

## Development Guidelines

### Branch Strategy
- **Main branch**: Submit pull requests directly to `main` for most contributions
- **Feature branches**: Create feature-specific branches for major new features
- **Naming**: Use descriptive branch names (e.g., `fix-sound-issue`, `add-html-support`)

### Code Standards
- Follow existing code style and conventions
- Write clear, self-documenting code
- Include appropriate comments for complex logic
- Maintain backward compatibility when possible

### Commit Guidelines
- Write clear and descriptive commit messages
- Use imperative mood (e.g., "Add feature" not "Added feature")
- Include detailed explanations of changes made
- Reference relevant issues when applicable

Example commit message:
```
Fix sound not playing for HTML center messages

- Ensure sound plays regardless of display type
- Add null check for sound configuration
- Update sound handling logic for center messages

Fixes #123
```

### Testing
- Test changes thoroughly on a development server
- Verify compatibility with different CS2 versions
- Test edge cases and error conditions
- Document any new configuration requirements

## Pull Request Process

### Before Submitting
1. **Fork the repository** and create your feature branch
2. **Test your changes** thoroughly
3. **Update documentation** if your changes affect configuration or usage
4. **Ensure code quality** and follow existing patterns

### Submitting Your PR
1. **Create a clear title** describing your changes
2. **Write a detailed description** including:
   - What changes were made and why
   - How to test the changes
   - Any breaking changes or migration steps
   - Related issues or discussions

3. **Link relevant issues** using keywords like "Fixes #123"

### PR Review Process
- Maintainers will review your PR and provide feedback
- Be responsive to comments and requested changes
- Updates to your PR will trigger additional reviews
- Once approved, your PR will be merged

## Code of Conduct

### Our Standards
- **Be respectful** and constructive in all interactions
- **Welcome newcomers** and help them get started
- **Focus on the code**, not the person
- **Accept feedback** gracefully and provide helpful reviews

### Unacceptable Behavior
- Harassment, discrimination, or offensive language
- Personal attacks or inflammatory comments
- Spam, trolling, or off-topic discussions
- Publishing private information without consent

## Getting Help

### Questions and Support
- **GitHub Discussions**: For general questions and discussions
- **GitHub Issues**: For bug reports and feature requests
- **Documentation**: Check the [complete documentation](https://www.lucauy.dev/docs/automaticads) first

### Development Setup
1. **Clone the repository**
2. **Set up a development environment** with CS2 server and CounterStrike Sharp
3. **Build and test** your changes locally
4. **Follow the contribution guidelines** outlined above

## Recognition

Contributors will be recognized in the following ways:
- **GitHub contributors** page shows all code contributors
- **Release notes** mention significant contributions
- **Documentation credits** for major documentation improvements

## License

By contributing to AutomaticAds CS2, you agree that your contributions will be licensed under the same license as the project.

---

## Thank You!

Every contribution, no matter how small, helps make AutomaticAds CS2 better for the entire Counter-Strike community. We appreciate your time and effort in improving this project!

For questions about contributing, feel free to reach out through GitHub Issues or Discussions.