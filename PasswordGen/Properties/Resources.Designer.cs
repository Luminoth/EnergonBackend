﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EnergonSoftware.PasswordGen.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("EnergonSoftware.PasswordGen.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Generate.
        /// </summary>
        public static string GenerateButtonLabel {
            get {
                return ResourceManager.GetString("GenerateButtonLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to EnergonSoftware Password Generator.
        /// </summary>
        public static string MainWindowTitle {
            get {
                return ResourceManager.GetString("MainWindowTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MD5:.
        /// </summary>
        public static string MD5Label {
            get {
                return ResourceManager.GetString("MD5Label", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Password:.
        /// </summary>
        public static string PasswordLabel {
            get {
                return ResourceManager.GetString("PasswordLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Realm:.
        /// </summary>
        public static string RealmLabel {
            get {
                return ResourceManager.GetString("RealmLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SHA512:.
        /// </summary>
        public static string SHA512Label {
            get {
                return ResourceManager.GetString("SHA512Label", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Username:.
        /// </summary>
        public static string UsernameLabel {
            get {
                return ResourceManager.GetString("UsernameLabel", resourceCulture);
            }
        }
    }
}
