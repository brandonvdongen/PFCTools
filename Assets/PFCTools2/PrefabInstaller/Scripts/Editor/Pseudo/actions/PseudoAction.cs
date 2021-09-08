namespace PFCTools2.Installer.PseudoParser {
    public abstract class PseudoAction {
        public abstract string ActionKey { get; }
        public virtual ControllerContext Process(ControllerContext Context, TokenStream Token) {
            return Context;
        }
    }
}