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
        static IBoardService boardService;
        
        public ViewModelLoader() 
        { 
            kinectService = new UserKinectService();
            //kinectService = new Debug.SimulatedKinectService();
            //sourceService = new Test.FileSourceService("eventstest.xml");
            sourceService = new WebSourceService();
            boardService = new MomentaryBoardService();
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
                    viewModelStatic = new ViewModel(kinectService, sourceService, boardService); 
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
            boardService.Cleanup();
        }
    }
}
