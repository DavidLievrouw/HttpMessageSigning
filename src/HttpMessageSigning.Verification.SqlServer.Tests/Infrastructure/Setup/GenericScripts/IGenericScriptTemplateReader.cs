namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.GenericScripts {
    internal interface IGenericScriptTemplateReader {
        string ReadCreateEmptyDatabaseScriptTemplate();
        string ReadDeleteDatabaseScriptTemplate();
    }
}