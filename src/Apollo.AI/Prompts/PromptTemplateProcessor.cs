namespace Apollo.AI.Prompts;

/// <summary>
/// Processes prompt templates by replacing placeholder variables with actual values.
/// </summary>
public interface IPromptTemplateProcessor
{
    /// <summary>
    /// Processes a template string by replacing placeholders with provided variable values.
    /// </summary>
    /// <param name="templateText">The template string containing placeholders like {variable_name}</param>
    /// <param name="variables">Dictionary of variable names and their replacement values</param>
    /// <returns>The processed template with all placeholders replaced</returns>
    string Process(string templateText, IDictionary<string, string> variables);
}

/// <summary>
/// Default implementation of prompt template processor using simple string replacement.
/// </summary>
public sealed class PromptTemplateProcessor : IPromptTemplateProcessor
{
    /// <inheritdoc />
    public string Process(string templateText, IDictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(templateText))
        {
            return templateText;
        }

        if (variables == null || variables.Count == 0)
        {
            return templateText;
        }

        var result = templateText;
        foreach (var (key, value) in variables)
        {
            var placeholder = $"{{{key}}}";
            result = result.Replace(placeholder, value ?? string.Empty);
        }

        return result;
    }
}
