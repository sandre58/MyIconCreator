# Conventional Commits Configuration

This project follows the [Conventional Commits](https://conventionalcommits.org/) specification for commit messages.

## 📝 Commit Message Format

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

## 🎯 Types

| Type | Description | Example |
|------|-------------|---------|
| `feat` | A new feature | `feat(auth): add JWT token validation` |
| `fix` | A bug fix | `fix(utils): resolve null reference exception` |
| `docs` | Documentation only changes | `docs(readme): update installation guide` |
| `style` | Changes that do not affect the meaning of the code | `style(format): fix indentation in UserService` |
| `refactor` | A code change that neither fixes a bug nor adds a feature | `refactor(auth): simplify token validation logic` |
| `perf` | A code change that improves performance | `perf(collections): optimize large list operations` |
| `test` | Adding missing tests or correcting existing tests | `test(utils): add unit tests for string helpers` |
| `build` | Changes that affect the build system or external dependencies | `build(deps): update NuGet packages` |
| `ci` | Changes to CI configuration files and scripts | `ci(github): add code coverage reporting` |
| `chore` | Other changes that don't modify src or test files | `chore(release): prepare version 1.2.0` |

## 🔧 Scopes

Common scopes for this project:

- `auth` - Authentication related changes
- `utils` - Utility functions and helpers
- `http` - HTTP client and related functionality
- `observable` - Observable collections and patterns
- `ui` - User interface components
- `wpf` - WPF-specific changes
- `avalonia` - Avalonia-specific changes
- `validation` - Input validation
- `logging` - Logging functionality
- `config` - Configuration management

## 📋 Examples

### ✅ Good Examples

```bash
feat(auth): add OAuth2 authentication support
fix(utils): resolve memory leak in string operations
docs(api): add documentation for new endpoints
refactor(http): simplify retry mechanism
perf(collections): improve ObservableCollection performance
test(validation): add comprehensive input validation tests
```

### ❌ Bad Examples

```bash
updated code
fixed bug
new feature
changes
WIP
```

## 🚨 Breaking Changes

For breaking changes, add `BREAKING CHANGE:` in the footer:

```bash
feat(api): remove deprecated authentication methods

BREAKING CHANGE: The old BasicAuth method has been removed. 
Use the new OAuth2Authentication method instead.
```

## 🏷️ Impact on Releases

- `feat` → Minor version bump (1.0.0 → 1.1.0)
- `fix` → Patch version bump (1.0.0 → 1.0.1)
- `BREAKING CHANGE` → Major version bump (1.0.0 → 2.0.0)

## 🛠️ Tools

This repository uses:
- **git-chglog** for automatic changelog generation
- **Nerdbank.GitVersioning** for automatic version calculation
- **GitHub Actions** for CI/CD and release automation

Following these conventions ensures:
- Automatic, meaningful changelogs
- Proper semantic versioning
- Better collaboration and code history
