using System;
using System.IO;
using UnityEditor;

namespace Neutron.Editor {
    internal static class PathUtils {
        public static string PackageRootDir {
            get {
                string packagesDir = "Packages/com.doji.neutron/";
                if (AssetDatabase.IsValidFolder(packagesDir)) {
                    return packagesDir;
                } else {
                    throw new Exception($"No valid package directory found.");
                }
            }
        }
        public static string ModelGraphEditorUxmlPath {
            get {
                return Path.Combine(PackageRootDir, "Editor", "ModelGraphEditor.uxml");
            }
        }
        public static string ModelGraphEditorUssPath {
            get {
                return Path.Combine(PackageRootDir, "Editor", "ModelGraphEditor.uss");
            }
        }
        public static string NodeViewUxml {
            get {
                return Path.Combine(PackageRootDir, "Editor", "NodeView.uxml");
            }
        }
    }
}