
slangc.exe "VisibilityMaskGenerator.slang" -I ./ -target spirv -enable-experimental-passes -fvk-use-entrypoint-name -o "VisibilityMaskGenerator.cs.spv" -reflection-json "VisibilityMaskGenerator.cs.spv.json"
slangc.exe "VisibilityMaskGenerator.slang" -I ./ -target spirv -enable-experimental-passes -fvk-use-entrypoint-name
