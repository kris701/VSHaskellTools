using Microsoft.VisualStudio.LanguageServer.Client;
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
        [Name("haskell")]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static Microsoft.VisualStudio.Utilities.ContentTypeDefinition HaskellContentTypeDefinition;

        [Export]
        [FileExtension(".hs")]
        [ContentType("haskell")]
        internal static FileExtensionToContentTypeDefinition HaskellFileExtensionDefinition;
    }
#pragma warning restore 649
}
