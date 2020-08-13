using Mono.Cecil;

namespace AssetStudio
{
    public class MyAssemblyResolver : DefaultAssemblyResolver
    {
        public void Register(AssemblyDefinition assembly)
        {
            RegisterAssembly(assembly);
        }
    }
}
