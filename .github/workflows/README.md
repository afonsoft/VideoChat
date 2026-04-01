# GitHub Workflows Configuration

## Overview

This repository uses automated GitHub workflows powered by Gemini AI for code review, vulnerability scanning, and PR management.

## Required Secrets

To enable these workflows, add the following secrets to your GitHub repository settings:

### 1. GEMINI_API_KEY
- **Description**: Google Gemini API key for AI-powered analysis
- **Required for**: PR reviews, vulnerability scanning, code analysis
- **How to get**: 
  1. Go to [Google AI Studio](https://aistudio.google.com/app/apikey)
  2. Create a new API key
  3. Copy the key and add to repository secrets

### 2. ALLHANDS_BOT_GITHUB_PAT
- **Description**: Personal Access Token for the bot account
- **Required for**: Creating PRs, commenting, assigning reviewers
- **Permissions needed**:
  - `repo` (Full repository access)
  - `pull-requests: write` (Create and manage PRs)
  - `issues: write` (Create comments)

## Available Workflows

### 🔍 PR Review by Gemini AI
**File**: `pr-review-by-openhands.yml`
**Triggers**: 
- New PR opened (non-draft)
- Draft PR marked ready for review
- 'review-this' label added
- Manual reviewer request

**Features**:
- Automated code review using Gemini 2.0 Flash
- Security vulnerability detection
- Code quality analysis
- Performance suggestions
- Auto-commenting with detailed feedback

### 📊 PR Review Evaluation
**File**: `pr-review-evaluation.yml`
**Triggers**: PR opened/updated
**Features**:
- Evaluates review completeness
- Tracks review effectiveness
- Assigns reviewers for first-time contributors
- Generates review metrics

### 🛡️ Vulnerability Remediation
**File**: `vulnerability-remediation-gemini.yml`
**Triggers**: 
- Manual workflow dispatch
- Daily scheduled scan (2 AM UTC)

**Features**:
- Automated vulnerability scanning
- AI-powered remediation suggestions
- Automatic PR creation for fixes
- Severity-based filtering (low, medium, high, critical)

### 👥 Auto Assign Reviews
**File**: `assign-reviews-gemini.yml`
**Triggers**: PR opened/ready_for_review
**Features**:
- Smart reviewer assignment based on code analysis
- Expertise matching
- Workload balancing
- Complexity-based reviewer selection

## Usage

### Manual PR Review
```bash
# Trigger manual review
gh workflow run pr-review-by-openhands.yml \
  --field vulnerability_report='path/to/report.json' \
  --field severity='high'
```

### Manual Vulnerability Scan
```bash
# Trigger manual scan
gh workflow run vulnerability-remediation-gemini.yml \
  --field vulnerability_report='path/to/report.json' \
  --field severity='critical'
```

## Configuration Options

### Review Styles
- `standard`: Standard code review with best practices
- `security`: Focus on security vulnerabilities
- `performance`: Focus on performance and optimization

### Severity Levels
- `low`: All vulnerabilities including minor issues
- `medium`: Medium and above (default)
- `high`: High and critical only
- `critical`: Critical vulnerabilities only

### Gemini Models Available
- `gemini/gemini-2.0-flash-exp`: Fast, efficient analysis (recommended)
- `gemini/gemini-2.0-flash-thinking`: Step-by-step reasoning
- `gemini/gemini-1.5-pro`: Most capable, slower

## Best Practices

### For Contributors
1. **Enable Reviews**: Allow automated reviews for faster feedback
2. **Respond to Comments**: Address AI-generated feedback promptly
3. **Quality PRs**: Well-documented PRs get better reviews

### For Maintainers
1. **Monitor Workflows**: Check workflow runs regularly
2. **Review AI Suggestions**: Validate AI recommendations
3. **Adjust Prompts**: Customize review criteria as needed
4. **Security**: Keep API keys secure and rotate regularly

## Troubleshooting

### Common Issues

#### Workflow Fails with "Secret not found"
- **Solution**: Add `GEMINI_API_KEY` and `ALLHANDS_BOT_GITHUB_PAT` to repository secrets

#### Review Comments Not Posted
- **Solution**: Check `ALLHANDS_BOT_GITHUB_PAT` permissions
- **Solution**: Verify bot account has repository access

#### Vulnerability Scan Fails
- **Solution**: Check Gemini API key validity and quota
- **Solution**: Verify npm audit dependencies

### Debug Mode

Enable debug logging by adding:
```yaml
env:
  DEBUG: true
```

## Integration with Development Tools

### VS Code
- Install [OpenHands VS Code Extension](https://marketplace.visualstudio.com/items?itemName=OpenHands.openhands-vscode)
- Local workflow testing
- Real-time preview

### CLI
```bash
# Install OpenHands CLI
npm install -g @openhands/sdk

# Test locally
openhands pr-review --help
openhands security-scan --help
```

## Security Considerations

- ✅ Workflows run in isolated environment
- ✅ No direct code execution from untrusted sources
- ✅ Secrets are encrypted and not logged
- ✅ Minimal permissions requested
- ✅ Regular security updates

## Support

For issues with workflows:
1. Check [Actions tab](https://github.com/FamilyMeet/VideoChat/actions) for workflow logs
2. Review [OpenHands documentation](https://docs.openhands.dev/)
3. Check [Gemini API documentation](https://ai.google.dev/docs)

## Future Enhancements

- [ ] Custom review rule configuration
- [ ] Integration with project management tools
- [ ] Multi-language support
- [ ] Performance benchmarking
- [ ] Code coverage integration
