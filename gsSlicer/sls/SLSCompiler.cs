namespace gs
{
    public interface ThreeAxisLaserCompiler
    {
        void Begin();

        void AppendPaths(ToolpathSet paths);

        void End();
    }

    public class SLSCompiler : ThreeAxisLaserCompiler
    {
        private SingleMaterialFFFSettings Settings;
        private IPathsAssembler Assembler;

        public SLSCompiler(SingleMaterialFFFSettings settings)
        {
            Settings = settings;
        }

        public virtual void Begin()
        {
            Assembler = InitializeAssembler();
        }

        // override to customize assembler
        protected virtual IPathsAssembler InitializeAssembler()
        {
            IPathsAssembler asm = new GenericPathsAssembler();
            return asm;
        }

        public virtual void End()
        {
        }

        public virtual void AppendPaths(ToolpathSet paths)
        {
            Assembler.AppendPaths(paths);
        }

        public ToolpathSet TempGetAssembledPaths()
        {
            return Assembler.TempGetAssembledPaths();
        }
    }
}