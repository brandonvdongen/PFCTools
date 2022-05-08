using PFCTools2.Utils;

namespace PFCTools2.Installer.PseudoParser {
    public abstract class PseudoAction {
        public abstract string ActionKey { get; }
        public virtual ControllerContext Process(ControllerContext context, TokenStream tokenStream, AvatarDefinition currentAvatar) {
            return context;
        }

        public virtual ControllerContext Remove(ControllerContext context, TokenStream tokenStream, AvatarDefinition currentAvatar) {
            while(!tokenStream.EOF() && tokenStream.Peek().type != TokenType.Action) {
                tokenStream.Next();
            }
            return context;
        }
    }
}