﻿using System;
using System.IO;
using System.Xml.Linq;
using ConfigSettings.Patch;
using ConfigSettings.Utils;
using FluentAssertions;
using NUnit.Framework;

namespace ConfigSettings.Tests
{
  [TestFixture]
  public class ConfigSettingsGetterTests
  {
    private readonly string tempPath = TestEnvironment.CreateRandomPath("ConfigSettingsGetter");

    [Test]
    public void WhenGetBooleanThenValueShoudBeTrue()
    {
      var configSettingsPath = this.CreateSettings(@"<var name=""SHOW_WELCOME_TEXT"" value=""true""/>");
      var getter = CreateConfigSettingsGetter(configSettingsPath);
      getter.Get<bool>("SHOW_WELCOME_TEXT").Should().BeTrue();
    }

    [Test]
    public void WhenGetUpperCaseBooleanThenValueShoudBeTrue()
    {
      var configSettingsPath = this.CreateSettings(@"<var name=""SHOW_WELCOME_TEXT"" value=""TRUE""/>");
      var getter = CreateConfigSettingsGetter(configSettingsPath);
      getter.Get<bool>("SHOW_WELCOME_TEXT").Should().BeTrue();
    }

    [Test]
    public void WhenGetBooleanFromEmptyStringThenValueShoudBeFalse()
    {
      var configSettingsPath = this.CreateSettings(@"<var name=""SHOW_WELCOME_TEXT"" value=""""/>");
      var getter = CreateConfigSettingsGetter(configSettingsPath);
      getter.Get<bool>("SHOW_WELCOME_TEXT").Should().BeFalse();
    }

    [Test]
    public void WhenGetUnexistingBooleanThenValueShoudBeFalse()
    {
      var configSettingsPath = this.CreateSettings(@"<var name=""SHOW_WELCOME_TEXT"" value=""""/>");
      var getter = CreateConfigSettingsGetter(configSettingsPath);
      getter.Get<bool>("UNEXISTED_BOOLEAN").Should().BeFalse();
    }

    [Test]
    public void WhenGetBooleanWithoutParserThenValueShoudBeFalse()
    {
      var getter = CreateNullConfigSettingsGetter();
      getter.Get<bool>("UNEXISTED_BOOLEAN").Should().BeFalse();
    }

    [Test]
    public void WhenChangeVariableThenBlockShouldNotChanged()
    {
      var configSettingsPath = this.CreateSettings(@"   <var name=""GIT_ROOT_DIRECTORY"" value=""d:\ee"" />
  <block name=""REPOSITORIES"">
    <repository folderName=""base"" solutionType=""Base"" url="""" />
    <repository folderName=""work"" solutionType=""Work"" url="""" />
  </block>");
      var getter = CreateConfigSettingsGetter(configSettingsPath);
      getter.Set("GIT_ROOT_DIRECTORY", "d:\\ee2");
      getter.Save();
      var content = this.GetConfigSettings(configSettingsPath);
      content.Should().Be(@"
  <var name=""GIT_ROOT_DIRECTORY"" value=""d:\ee2"" />
  <block name=""REPOSITORIES"">
    <repository folderName=""base"" solutionType=""Base"" url="""" />
    <repository folderName=""work"" solutionType=""Work"" url="""" />
  </block>
");
    }

    [Test]
    public void WhenSetEmptyBlockContent()
    {
      var configSettingsPath = this.CreateSettings(@"  <block name=""REPOSITORIES"">
  </block>");
      var getter = CreateConfigSettingsGetter(configSettingsPath);
      getter.SetBlock("TESTBLOCK", null, null);
      getter.Save();
      var content = this.GetConfigSettings(configSettingsPath);
      content.Should().Be(@"
  <block name=""REPOSITORIES""></block>
  <block name=""TESTBLOCK""></block>
");
    }

    [Test]
    public void WhenSetEmptyBlockEnabled()
    {
      var configSettingsPath = this.CreateSettings(@"  
  <block name=""ORIGIN_TRUE_BLOCK"" enabled=""true""/>
  <block name=""ORIGIN_FALSE_BLOCK"" enabled=""false""/>
  <block name=""ORIGIN_NULL_BLOCK""/>
");
      var getter = CreateConfigSettingsGetter(configSettingsPath);
      getter.SetBlock("TEST_TRUE_BLOCK", true, null);
      getter.SetBlock("TEST_FALSE_BLOCK", false, null);
      getter.SetBlock("TEST_NULL_BLOCK", null, null);
      getter.Save();
      var content = this.GetConfigSettings(configSettingsPath);
      content.Should().Be(@"
  <block name=""ORIGIN_TRUE_BLOCK"" enabled=""True""></block>
  <block name=""ORIGIN_FALSE_BLOCK"" enabled=""False""></block>
  <block name=""ORIGIN_NULL_BLOCK""></block>
  <block name=""TEST_TRUE_BLOCK"" enabled=""True""></block>
  <block name=""TEST_FALSE_BLOCK"" enabled=""False""></block>
  <block name=""TEST_NULL_BLOCK""></block>
");
    }

    [Test]
    public void WhenSetBlockValueAndEnabled()
    {
      var configSettingsPath = this.CreateSettings(@"  
  <block name=""ORIGIN_TRUE_BLOCK"" enabled=""false"">
    <repository folderName=""base"" solutionType=""Base"" url="""" />
    <repository folderName=""work"" solutionType=""Work"" url="""" />
</block>
");
      var getter = CreateConfigSettingsGetter(configSettingsPath);
      getter.SetBlock("TEST_TRUE_BLOCK", true, @"
  <testRepository folderName=""base"" solutionType=""Base"" url="""" />
  <testRepository folderName=""work"" solutionType=""Work"" url="""" />");
      getter.Save();
      var content = this.GetConfigSettings(configSettingsPath);
      content.Should().Be(@"
  <block name=""ORIGIN_TRUE_BLOCK"" enabled=""false"">
    <repository folderName=""base"" solutionType=""Base"" url="""" />
    <repository folderName=""work"" solutionType=""Work"" url="""" />
  </block>
  <block name=""TEST_TRUE_BLOCK"" enabled=""True"">
    <testRepository folderName=""base"" solutionType=""Base"" url="""" />
    <testRepository folderName=""work"" solutionType=""Work"" url="""" />
  </block>
");
    }

    [Test]
    public void WhenSetRelativeImportThenPathShouldNotBeAbsolute()
    {
      var configSettingsPath = this.CreateSettings(@"  
  <import from=""origin/import/from"" />
");

      var getter = CreateConfigSettingsGetter(configSettingsPath);
      getter.SetImport("test/import/from");
      getter.Save();
      var content = this.GetConfigSettings(configSettingsPath);
      content.Should().Be(@"
  <import from=""origin/import/from"" />
  <import from=""test/import/from"" />
");
    }

    [Test]
    public void WhenParseEmptyImportAndAddVariableThenImportBlockShouldNotBeSaved()
    {
      var configSettingsPath = this.CreateSettings(@"  
  <import from="""" />
");
      var getter = CreateConfigSettingsGetter(configSettingsPath);
      getter.Set("testName", "testValue");
      getter.Save();
      var content = this.GetConfigSettings(configSettingsPath);
      content.Should().Be(@"
  <var name=""testName"" value=""testValue"" />
");
    }

    private static ConfigSettingsGetter CreateConfigSettingsGetter(string configSettingsPath)
    {
      return new ConfigSettingsGetter(new ConfigSettingsParser(configSettingsPath, XDocument.Load(configSettingsPath)));
    }

    private static ConfigSettingsGetter CreateNullConfigSettingsGetter()
    {
      return new ConfigSettingsGetter(null);
    }

    private string CreateSettings(string settings)
    {
      var content = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<settings>
{settings}
</settings>";
      var fileName = Path.Combine(this.tempPath, $@"test_settings_{Guid.NewGuid().ToShortString()}.xml");
      File.WriteAllText(fileName, content);
      return fileName;
    }

    public string GetConfigSettings(string configPath)
    {
      var content = File.ReadAllText(configPath);
      return content.Replace(@"<?xml version=""1.0"" encoding=""utf-8""?>
<settings>", string.Empty).Replace("</settings>", string.Empty);
    }
  }
}