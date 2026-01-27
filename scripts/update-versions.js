#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

// Read current versions from Directory.Build.props
function readCurrentVersions() {
  const buildPropsPath = path.join(__dirname, '../Directory.Build.props');
  const content = fs.readFileSync(buildPropsPath, 'utf8');
  
  const coreVersion = content.match(/<CorePackageVersion>([^<]+)<\/CorePackageVersion>/)[1];
  const generatorVersion = content.match(/<GeneratorPackageVersion>([^<]+)<\/GeneratorPackageVersion>/)[1];
  
  return { coreVersion, generatorVersion };
}

// Update versions in Directory.Build.props
function updateVersions(newCoreVersion, newGeneratorVersion) {
  const buildPropsPath = path.join(__dirname, '../Directory.Build.props');
  let content = fs.readFileSync(buildPropsPath, 'utf8');
  
  content = content.replace(
    /<CorePackageVersion>[^<]+<\/CorePackageVersion>/,
    `<CorePackageVersion>${newCoreVersion}</CorePackageVersion>`
  );
  
  content = content.replace(
    /<GeneratorPackageVersion>[^<]+<\/GeneratorPackageVersion>/,
    `<GeneratorPackageVersion>${newGeneratorVersion}</GeneratorPackageVersion>`
  );
  
  fs.writeFileSync(buildPropsPath, content);
  console.log(`✅ Updated versions:`);
  console.log(`   Core: ${newCoreVersion}`);
  console.log(`   Generator: ${newGeneratorVersion}`);
}

// Parse version bump type from command line
function getBumpType() {
  const args = process.argv.slice(2);
  return args[0] || 'patch';
}

// Calculate next versions based on bump type
function getNextVersions(currentCore, currentGenerator, bumpType) {
  const [coreMajor, coreMinor, corePatch] = currentCore.split('.').map(Number);
  const [genMajor, genMinor, genPatch] = currentGenerator.split('.').map(Number);
  
  switch (bumpType) {
    case 'major':
      return {
        core: `${coreMajor + 1}.0.0`,
        generator: `1.0.0` // Generator always starts fresh at major releases
      };
    case 'minor':
      return {
        core: `${coreMajor}.${coreMinor + 1}.0`,
        generator: `1.${genMinor + 1}.0` // Generator increments its own minor version
      };
    case 'patch':
      return {
        core: `${coreMajor}.${coreMinor}.${corePatch + 1}`,
        generator: `1.${genMinor}.${genPatch + 1}` // Generator increments its own patch version
      };
    default:
      throw new Error(`Unknown bump type: ${bumpType}`);
  }
}

// Main execution
function main() {
  try {
    const bumpType = getBumpType();
    const current = readCurrentVersions();
    const next = getNextVersions(current.coreVersion, current.generatorVersion, bumpType);
    
    console.log(`📦 Current versions:`);
    console.log(`   Core: ${current.coreVersion}`);
    console.log(`   Generator: ${current.generatorVersion}`);
    console.log(``);
    console.log(`🚀 Bumping: ${bumpType}`);
    
    updateVersions(next.core, next.generator);
    
    console.log(``);
    console.log(`✨ Version update complete!`);
    console.log(`📝 Don't forget to commit these changes.`);
    
  } catch (error) {
    console.error(`❌ Error updating versions:`, error.message);
    process.exit(1);
  }
}

if (require.main === module) {
  main();
}

module.exports = { readCurrentVersions, updateVersions, getNextVersions };
