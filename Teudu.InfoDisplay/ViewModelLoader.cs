using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using Microsoft.Practices.Unity;
using System.Configuration;

namespace Teudu.InfoDisplay
{
    public class ViewModelLoader    
    {
        IUnityContainer unityContainer;
        static ViewModel viewModelStatic; 
        static IKinectService kinectService;
        static ISourceService sourceService;
        static IBoardService boardService;
        static IHelpService helpService;
        
        public ViewModelLoader() 
        {
            unityContainer = new UnityContainer();

            RegisterTypes();

            string kinectSetting = ConfigurationManager.AppSettings["KinectService"].ToString();
            string sourceSetting = ConfigurationManager.AppSettings["SourceService"].ToString();
            string boardSetting = ConfigurationManager.AppSettings["BoardService"].ToString();
            string helpSetting = ConfigurationManager.AppSettings["HelpService"].ToString();


            kinectService = unityContainer.Resolve<IKinectService>(kinectSetting);
            sourceService = unityContainer.Resolve<ISourceService>(sourceSetting);
            boardService = unityContainer.Resolve<IBoardService>(boardSetting);
            helpService = unityContainer.Resolve<IHelpService>(helpSetting);

            var prop = DesignerProperties.IsInDesignModeProperty; 
            var isInDesignMode = (bool)DependencyPropertyDescriptor
                .FromProperty(prop, typeof(FrameworkElement))
                .Metadata.DefaultValue; 
            
            if (!isInDesignMode) 
            { 
                kinectService.Initialize();
                sourceService.Initialize();
                helpService.Initialize();
            } 
        }

        /// <summary>
        /// Registers custom modules with labels that can be set in app.config
        /// </summary>
        private void RegisterTypes()
        {
            unityContainer.RegisterType<IKinectService, UserKinectService>("Default");
            unityContainer.RegisterType<IKinectService, UserKinectService>("Kinect");
            unityContainer.RegisterType<IKinectService, Debug.SimulatedKinectService>("Simulated");

            unityContainer.RegisterType<ISourceService, WebSourceService>("Default");
            unityContainer.RegisterType<ISourceService, WebSourceService>("Web");
            unityContainer.RegisterType<ISourceService, FileSourceService>("File");

            unityContainer.RegisterType<IBoardService, MomentaryBoardService>("Default");
            unityContainer.RegisterType<IBoardService, MomentaryBoardService>("Momentary");

            unityContainer.RegisterType<IHelpService, InstructionalHelpService>("Default");
            unityContainer.RegisterType<IHelpService, InstructionalHelpService>("Instructional");
        }
        
        public static ViewModel ViewModelStatic 
        { 
            get 
            { 
                if (viewModelStatic == null) 
                { 
                    viewModelStatic = new ViewModel(kinectService, sourceService, boardService, helpService); 
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
            helpService.Cleanup();
        }
    }
}
