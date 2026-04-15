

echo ""
echo ""
echo "------------------------"
echo "Counting engine code..."
echo "------------------------"
echo ""
cloc src --quiet --hide-rate --exclude-lang=XML,C++,C --exclude-dir=.git,.idea,.kdev4,.vs,.vscode,bin,obj,Elegy.Game

echo ""
echo ""
echo "------------------------"
echo "Counting game code..."
echo "------------------------"
echo ""
cloc src/Plugins/Elegy.Game --quiet --hide-rate --exclude-lang=XML,C++,C --exclude-dir=.git,.idea,.kdev4,.vs,.vscode,bin,obj

echo ""
echo ""
echo "------------------------"
echo "Counting game configs and stuff..."
echo "------------------------"
echo ""
cloc testgame --quiet --hide-rate --exclude-lang=XML,C++,C --exclude-dir=.git,.idea,.kdev4,.vs,.vscode,obj

echo ""
echo ""
echo "------------------------"
echo "Counting Elegy.Veldrid..."
echo "------------------------"
echo ""
cloc extern/Elegy.Veldrid --quiet --hide-rate --exclude-lang=XML,C++,C --exclude-dir=.git,.idea,.kdev4,.vs,.vscode,bin,obj,veldrid-spirv,NeoDemo,Veldrid.MetalBindings,Veldrid.OpenGLBindings,Veldrid.SDL2,Veldrid.StartupUtilities,Veldrid.Tests,Veldrid.Tests.Android,Veldrid.VirtualReality,Veldrid.VirtualRealitySample,Android,D3D11,MTL,OpenGL

