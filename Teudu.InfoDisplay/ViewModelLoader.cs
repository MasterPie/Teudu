using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;

namespace Teudu.InfoDisplay
{
    public class ViewModelLoader
    {
        static ViewModel viewModelStatic; 
        static IKinectService kinectService; 
        
        public ViewModelLoader() 
        { 
            kinectService = new UserKinectService(); 
            var prop = DesignerProperties.IsInDesignModeProperty; 
            var isInDesignMode = (bool)DependencyPropertyDescriptor
                .FromProperty(prop, typeof(FrameworkElement))
                .Metadata.DefaultValue; 
            
            if (!isInDesignMode) 
            { 
                kinectService.Initialize(); 
            } 
        }        
        
        public static ViewModel ViewModelStatic 
        { 
            get 
            { 
                if (viewModelStatic == null) 
                { 
                    viewModelStatic = new ViewModel(kinectService); 
                } 
                return viewModelStatic; 
            } 
        }        
        
        public ViewModel ViewModel 
        { 
            get { return ViewModelStatic; } 
        }        
        
        public static void Cleanup() 
        { 
            if (viewModelStatic != null) 
            { 
                viewModelStatic.Cleanup(); 
            } 
            kinectService.Cleanup(); 
        }
    }
}
