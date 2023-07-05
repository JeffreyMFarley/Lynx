using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Msagl;
using Microsoft.Msagl.Splines;
using System.Collections.Generic;
using P2 = Microsoft.Msagl.Point;

namespace Lynx.UI.AGLExtensions
{
    //// This is public so we can serialize the XAML.
    //public class EdgeShape : SelectableShape, Microsoft.Msagl.Drawing.IViewerEdge, Microsoft.Msagl.Drawing.IEditableObject
    //{

    //    internal void Invalidate() { }


    //    public double StrokeThickness
    //    {
    //        get { return Stroke.Thickness; }
    //        set { Stroke.Thickness = value; }
    //    }

    //    System.Windows.FrameworkElement label;

    //    public System.Windows.FrameworkElement Label
    //    {
    //        get { return label; }
    //        set { label = value; }
    //    }

    //    const double arrowAngle = 30.0;//degrees

    //    NodeShape source;

    //    public Microsoft.Msagl.Drawing.IViewerNode Source
    //    {
    //        get { return source; }
    //        set { source = (NodeShape)value; }
    //    }

    //    NodeShape target;

    //    public Microsoft.Msagl.Drawing.IViewerNode Target
    //    {
    //        get { return target; }
    //        set { target = (NodeShape)value; }
    //    }

    //    ICurve Curve
    //    {
    //        get { return this.geometryEdge.Curve; }
    //    }
    //    Microsoft.Msagl.Drawing.Edge drawingEdge;
    //    Microsoft.Msagl.Edge geometryEdge;

    //    public EdgeShape()
    //    {
    //        this.Focusable = false;
    //    }

    //    internal EdgeShape(Microsoft.Msagl.Drawing.Edge drawingEdgePar, Microsoft.Msagl.Edge geomEdge)
    //    {
    //        this.Focusable = false;

    //        drawingEdge = drawingEdgePar;

    //        this.geometryEdge = geomEdge;

    //        SetStrokeFill();

    //        Pen pen = this.Stroke;

    //        if (this.Stroke == null)
    //            this.Stroke = new Pen();

    //        Stroke.Thickness = LayoutAlgorithmSettings.PointSize * drawingEdge.Attr.LineWidth;

    //        foreach (Microsoft.Msagl.Drawing.Style style in drawingEdge.Attr.Styles)
    //        {
    //            if (style == Microsoft.Msagl.Drawing.Style.Dotted)
    //                pen.DashStyle = DashStyles.Dot;
    //            else if (style == Microsoft.Msagl.Drawing.Style.Dashed)
    //                pen.DashStyle = DashStyles.Dash;
    //        }


    //        drawingEdgePar.Attr.LineWidthHasChanged += new EventHandler(Attr_LineWidthHasChanged);

    //    }

    //    void Attr_LineWidthHasChanged(object sender, EventArgs e)
    //    {
    //        if (Stroke != null)
    //            Stroke.Thickness = drawingEdge.Attr.LineWidth * LayoutAlgorithmSettings.PointSize;
    //    }

    //    static Brush selectedStroke = new SolidColorBrush(Colors.Blue);
    //    static Brush highlightedStroke = new SolidColorBrush(Colors.Red);

    //    override public void SetStrokeFill()
    //    {
    //        Brush fillBrush = null;
    //        Brush strokeBrush = null;
    //        if (Highlighted)
    //        {
    //            fillBrush = strokeBrush = highlightedStroke;
    //        }
    //        else if (Selected)
    //        {
    //            fillBrush = strokeBrush = selectedStroke;
    //        }
    //        else
    //        {
    //            fillBrush = strokeBrush = Common.BrushFromMsaglColor(drawingEdge.Attr.Color);
    //        }
    //        this.Fill = fillBrush;

    //        Pen p = this.Stroke;

    //        if (this.Stroke == null)
    //            this.Stroke = new Pen();

    //        Stroke.Brush = strokeBrush;
    //        Stroke.Thickness = LayoutAlgorithmSettings.PointSize * drawingEdge.Attr.LineWidth;
    //    }

    //    public Microsoft.Msagl.Drawing.Edge Edge
    //    {
    //        get { return drawingEdge; }
    //    }

    //    public Microsoft.Msagl.Drawing.DrawingObject DrawingObject
    //    {
    //        get { return Edge; }
    //    }

    //    public override Geometry DefiningGeometry
    //    {
    //        get
    //        {

    //            StreamGeometry streamGeometry = new StreamGeometry();
    //            using (StreamGeometryContext context = streamGeometry.Open())
    //            {

    //                FillStreamGeometryContext(context);

    //                return streamGeometry;
    //            }
    //        }
    //    }



    //    private void FillStreamGeometryContext(StreamGeometryContext context)
    //    {
    //        context.BeginFigure(Point(Curve.Start), false, false);

    //        Curve c = Curve as Curve;
    //        if (c != null)
    //            foreach (ICurve seg in c.Segments)
    //            {
    //                CubicBezierSegment bezSeg = seg as CubicBezierSegment;
    //                if (bezSeg != null)
    //                {
    //                    context.BezierTo(Point(bezSeg.B(1)),
    //                               Point(bezSeg.B(2)), Point(bezSeg.B(3)), true, false);

    //                }
    //                else
    //                {
    //                    Microsoft.Msagl.Splines.LineSegment ls = seg as Microsoft.Msagl.Splines.LineSegment;
    //                    if (ls != null)
    //                        context.LineTo(Point(ls.End), true, false);
    //                    else
    //                        throw new InvalidOperationException();
    //                }
    //            }
    //        else
    //        {
    //            CubicBezierSegment cubicBezierSeg = Curve as CubicBezierSegment;
    //            if (cubicBezierSeg != null)
    //                context.BezierTo(Point(cubicBezierSeg.B(1)), Point(cubicBezierSeg.B(2)), Point(cubicBezierSeg.B(3)), true, false);
    //            else
    //                throw new InvalidOperationException();
    //        }

    //        if (geometryEdge.ArrowheadAtSource)
    //        {
    //            Point start = Curve.Start;
    //            Point end = geometryEdge.ArrowheadAtSourcePosition;
    //            AddArrow(context, ref start, ref end);
    //        }


    //        if (geometryEdge.ArrowheadAtTarget)
    //        {
    //            Point start = Curve.End;
    //            Point end = geometryEdge.ArrowheadAtTargetPosition;
    //            AddArrow(context, ref start, ref end);
    //        }

    //        if (Label != null)
    //            Diagram.SetLabelPosition(Label);

    //        if (this.SelectedForEditing)
    //        {
    //            VisualizeUnderlyingPolyline(context);
    //        }

    //    }

    //    private void VisualizeUnderlyingPolyline(StreamGeometryContext context)
    //    {
    //        IEnumerator<Point> en = this.Edge.Attr.GeometryEdge.UnderlyingPolyline.GetEnumerator();
    //        en.MoveNext();
    //        context.BeginFigure(Point(en.Current), false, false);
    //        while (en.MoveNext())
    //            context.LineTo(Point(en.Current), true, false);

    //        Point d0 = new Point(RadiusOfPolylineCorner, 0);

    //        foreach (Microsoft.Msagl.Point p in this.Edge.Attr.GeometryEdge.UnderlyingPolyline)
    //        {
    //            context.BeginFigure(Point(p + d0), false, true);
    //            context.ArcTo(Point(p - d0), new System.Windows.Size(RadiusOfPolylineCorner, RadiusOfPolylineCorner), 0, false, SweepDirection.Clockwise, true, false);
    //            context.ArcTo(Point(p + d0), new System.Windows.Size(RadiusOfPolylineCorner, RadiusOfPolylineCorner), 0, false, SweepDirection.Clockwise, true, false);
    //        }
    //    }

    //    private double FindGoodCircleRadius()
    //    {
    //        //this code has to be changed with the change of the tree
    //        Diagram diagram = (this.Parent as Canvas).Parent as Diagram;
    //        return diagram.MouseHitTolerance;
    //    }

    //    private void AddArrow(StreamGeometryContext context, ref Point start, ref Point end)
    //    {
    //        if (drawingEdge.Attr.LineWidth > 1)
    //        {
    //            Point dir = end - start;
    //            Point h = dir;
    //            dir /= dir.Length;

    //            Point s = new Point(-dir.Y, dir.X);
    //            double w = 0.5 * LayoutAlgorithmSettings.PointSize * drawingEdge.Attr.LineWidth;
    //            Point s0 = w * s;
    //            double al = arrowAngle * 0.5 * (Math.PI / 180.0);
    //            s *= h.Length * Math.Tan(al);
    //            s += s0;

    //            Point center = end - dir * w * Math.Tan(al);
    //            double rad = w / (double)Math.Cos(al);

    //            context.BeginFigure(Common.Point(start + s), true, true);
    //            context.LineTo(Common.Point(start - s), true, false);
    //            context.LineTo(Common.Point(end - s0), true, false);
    //            context.ArcTo(Common.Point(end + s0), new System.Windows.Size(rad, rad), Math.PI - 2 * al, false, SweepDirection.Clockwise, true, false);
    //        }
    //        else
    //        {

    //            Point dir = end - start;

    //            Point h = dir;
    //            dir /= dir.Length;
    //            end -= dir * LayoutAlgorithmSettings.PointSize * 2;


    //            Point s = new Point(-dir.Y, dir.X);

    //            s *= h.Length * ((double)Math.Tan(arrowAngle * 0.5f * (Math.PI / 180.0)));

    //            context.BeginFigure(Common.Point(start + s), true, true);
    //            context.LineTo(Common.Point(end), true, false);
    //            context.LineTo(Common.Point(start - s), true, false);

    //        }
    //    }

    //    System.Windows.Point Point(Point p2)
    //    {
    //        return new System.Windows.Point(p2.X, p2.Y);
    //    }


    //    #region IDraggableEdge Members


    //    public Microsoft.Msagl.Splines.Rectangle BoundingBox
    //    {
    //        get { throw new Exception("The method or operation is not implemented."); }
    //    }

    //    double radiusOfPolylineCorner;
    //    /// <summary>
    //    ///the radius of circles drawin around polyline corners 
    //    /// </summary>
    //    public double RadiusOfPolylineCorner
    //    {
    //        get
    //        {
    //            return radiusOfPolylineCorner;
    //        }
    //        set
    //        {
    //            radiusOfPolylineCorner = value;
    //        }
    //    }

    //    #endregion

    //    #region IEditableObject Members

    //    bool selectedForEditing;

    //    public bool SelectedForEditing
    //    {
    //        get
    //        {
    //            return selectedForEditing;
    //        }
    //        set
    //        {
    //            selectedForEditing = value;
    //        }
    //    }

    //    #endregion

    //}
}