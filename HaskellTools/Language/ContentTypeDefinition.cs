using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaskellTools
{
#pragma warning disable 649
    public class ContentTypeDefinition
    {
        [Export]
        [Name(Constants.HaskellLanguageName)]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteTextMateBraceCompletionTypeName)]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteTextMateStructureTypeName)]
        internal static Microsoft.VisualStudio.Utilities.ContentTypeDefinition HaskellContentTypeDefinition;

        [Export]
        [FileExtension(Constants.HaskellExt)]
        [ContentType(Constants.HaskellLanguageName)]
        internal static FileExtensionToContentTypeDefinition HaskellFileExtensionDefinition;
    }
#pragma warning restore 649
}
