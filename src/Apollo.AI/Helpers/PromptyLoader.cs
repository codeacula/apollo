#pragma warning disable SKEXP0040 // Type is for evaluation purposes only

using Microsoft.SemanticKernel.Prompty;

namespace Apollo.AI.Helpers;

public static class PromptyLoader
{
  public static string LoadSystemPromptFromFile(string filePath)
  {
    if (!File.Exists(filePath))
    {
      throw new FileNotFoundException($"Prompty file not found at path: {filePath}");
    }

    var fileContent = File.ReadAllText(filePath);

    // Use the Prompty API to parse the file properly
    var promptConfig = KernelFunctionPrompty.ToPromptTemplateConfig(fileContent);

    if (string.IsNullOrWhiteSpace(promptConfig.Template))
    {
      throw new InvalidOperationException("Prompty file does not contain a valid template");
    }

    // The Template property contains the prompt template in format "system:\n<content>"
    // We need to extract just the content part for use as a system message
    var template = promptConfig.Template.Trim();

    // Remove the "system:" prefix if present
    if (template.StartsWith("system:", StringComparison.OrdinalIgnoreCase))
    {
      template = template["system:".Length..].Trim();
    }

    return template;
  }
}

#pragma warning restore SKEXP0040
