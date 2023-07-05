using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl;
using Microsoft.Msagl.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using LYNX = Lynx.Models;

namespace Lynx.UI.AGLExtensions
{
    public class GraphHost
    {
        public FrameworkElement Element
        {
            get
            {
                if (element == null)
                {
                    element = new WindowsFormsHost();
                    element.Child = ViewControl;
                }
                return element;
            }
            set
            {
                element = value as WindowsFormsHost;
            }
        }
        WindowsFormsHost element;

        public GViewer ViewControl
        {
            get
            {
                if (viewControl == null)
                {
                    viewControl = new GViewer();
                    
                }
                return viewControl;
            }
            set
            {
                viewControl = value;
            }
        }
        GViewer viewControl;

        public void Render(LYNX.LinkSet links)
        {
            var graph = new Graph();
            graph.Attr.LayerDirection = LayerDirection.LR;

            foreach (LYNX.LinkAsEdge link in links)
            {
                graph.AddEdge(link.Source.Name, link.Target.Name);
            }

            ViewControl.Graph = graph;
        }
    }
}
