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
        static ISourceService sourceService;
        
        public ViewModelLoader() 
        { 
            kinectService = new UserKinectService();
            sourceService = new Test.FileSourceService("eventstest.xml");
            var prop = DesignerProperties.IsInDesignModeProperty; 
            var isInDesignMode = (bool)DependencyPropertyDescriptor
                .FromProperty(prop, typeof(FrameworkElement))
                .Metadata.DefaultValue; 
            
            if (!isInDesignMode) 
            { 
                kinectService.Initialize();
                sourceService.Initialize();
            } 
        }        
        
        public static ViewModel ViewModelStatic 
        { 
            get 
            { 
                if (viewModelStatic == null) 
                { 
                    viewModelStatic = new ViewModel(kinectService, sourceService); 
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
            sourceService.Cleanup();
        }
    }
}
