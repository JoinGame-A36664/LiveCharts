//The MIT License(MIT)

//Copyright(c) 2016 Alberto Rodriguez & LiveCharts Contributors

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using LiveCharts.Definitions.Points;
using LiveCharts.Definitions.Series;
using LiveCharts.SeriesAlgorithms;
using LiveCharts.Uwp.Charts.Base;
using LiveCharts.Uwp.Components;
using LiveCharts.Uwp.Points;

namespace LiveCharts.Uwp
{
    /// <summary>
    /// The Candle series defines a financial series, add this series to a cartesian chart
    /// </summary>
    public class CandleSeries : Series, IFinancialSeriesView
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of OhclSeries class
        /// </summary>
        public CandleSeries()
        {
            Model = new CandleAlgorithm(this);
            InitializeDefuaults();
        }

        /// <summary>
        /// Initializes a new instance of OhclSeries class with a given mapper
        /// </summary>
        /// <param name="configuration"></param>
        public CandleSeries(object configuration)
        {
            Model = new CandleAlgorithm(this);
            Configuration = configuration;
            InitializeDefuaults();
        }

        #endregion

        #region Private Properties

        #endregion

        #region Properties

        /// <summary>
        /// The maximum column width property
        /// </summary>
        public static readonly DependencyProperty MaxColumnWidthProperty = DependencyProperty.Register(
            "MaxColumnWidth", typeof (double), typeof (CandleSeries), new PropertyMetadata(35d));
        /// <summary>
        /// Gets or sets the maximum with of a point, a point will be capped to this width.
        /// </summary>
        public double MaxColumnWidth
        {
            get { return (double) GetValue(MaxColumnWidthProperty); }
            set { SetValue(MaxColumnWidthProperty, value); }
        }

        /// <summary>
        /// The increase brush property
        /// </summary>
        public static readonly DependencyProperty IncreaseBrushProperty = DependencyProperty.Register(
            "IncreaseBrush", typeof (Brush), typeof (CandleSeries), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 254, 178, 0))));
        /// <summary>
        /// Gets or sets the brush of the point when close value is grater than open value
        /// </summary>
        public Brush IncreaseBrush
        {
            get { return (Brush) GetValue(IncreaseBrushProperty); }
            set { SetValue(IncreaseBrushProperty, value); }
        }

        /// <summary>
        /// The decrease brush property
        /// </summary>
        public static readonly DependencyProperty DecreaseBrushProperty = DependencyProperty.Register(
            "DecreaseBrush", typeof (Brush), typeof (CandleSeries), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 238, 83, 80))));
        /// <summary>
        /// Gets or sets the brush of the point when close value is less than open value
        /// </summary>
        public Brush DecreaseBrush
        {
            get { return (Brush) GetValue(DecreaseBrushProperty); }
            set { SetValue(DecreaseBrushProperty, value); }
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// This method runs when the update starts
        /// </summary>
        public override void OnSeriesUpdateStart()
        {
            //do nothing on updateStart
        }

        /// <summary>
        /// Gets the view of a given point
        /// </summary>
        /// <param name="point"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public override IChartPointView GetPointView(ChartPoint point, string label)
        {
            var pbv = (CandlePointView) point.View;

            if (pbv == null)
            {
                pbv = new CandlePointView
                {
                    IsNew = true,
                    HighToLowLine = new Line(),
                    OpenToCloseRectangle = new Rectangle()
                };

                Model.Chart.View.AddToDrawMargin(pbv.HighToLowLine);
                Model.Chart.View.AddToDrawMargin(pbv.OpenToCloseRectangle);
            }
            else
            {
                pbv.IsNew = false;
                point.SeriesView.Model.Chart.View
                    .EnsureElementBelongsToCurrentDrawMargin(pbv.HighToLowLine);
                point.SeriesView.Model.Chart.View
                    .EnsureElementBelongsToCurrentDrawMargin(pbv.OpenToCloseRectangle);
                point.SeriesView.Model.Chart.View
                    .EnsureElementBelongsToCurrentDrawMargin(pbv.HoverShape);
                point.SeriesView.Model.Chart.View
                    .EnsureElementBelongsToCurrentDrawMargin(pbv.DataLabel);
            }

            var i = Canvas.GetZIndex(this);

            pbv.HighToLowLine.StrokeThickness = StrokeThickness;
            pbv.HighToLowLine.StrokeDashArray = StrokeDashArray;
            pbv.HighToLowLine.Visibility = Visibility;
            Canvas.SetZIndex(pbv.HighToLowLine, i);

            pbv.OpenToCloseRectangle.Fill = Fill;
            pbv.OpenToCloseRectangle.StrokeThickness = StrokeThickness;
            pbv.OpenToCloseRectangle.Stroke = Stroke;
            pbv.OpenToCloseRectangle.StrokeDashArray = StrokeDashArray;
            pbv.OpenToCloseRectangle.Visibility = Visibility;
            Canvas.SetZIndex(pbv.HighToLowLine, i);

            if (Model.Chart.RequiresHoverShape && pbv.HoverShape == null)
            {
                pbv.HoverShape = new Rectangle
                {
                    Fill = new SolidColorBrush(Colors.Transparent),
                    StrokeThickness = 0
                };

                Canvas.SetZIndex(pbv.HoverShape, short.MaxValue);

                var uwpfChart = (Chart)Model.Chart.View;
                uwpfChart.AttachHoverableEventTo(pbv.HoverShape);

                Model.Chart.View.AddToDrawMargin(pbv.HoverShape);
            }

            if (pbv.HoverShape != null) pbv.HoverShape.Visibility = Visibility;

            if (DataLabels)
            {
                pbv.DataLabel = UpdateLabelContent(new DataLabelViewModel
                {
                    FormattedText = label,
                    Instance = point.Instance
                }, pbv.DataLabel);
            }

            if (point.Open <= point.Close)
            {
                pbv.HighToLowLine.Stroke = IncreaseBrush;
                pbv.OpenToCloseRectangle.Fill = IncreaseBrush;
                pbv.OpenToCloseRectangle.Stroke = IncreaseBrush;
            }
            else
            {
                pbv.HighToLowLine.Stroke = DecreaseBrush;
                pbv.OpenToCloseRectangle.Fill = DecreaseBrush;
                pbv.OpenToCloseRectangle.Stroke = DecreaseBrush;
            }

            return pbv;
        }

        #endregion

        #region Private Methods

        private void InitializeDefuaults()
        {
            Func<ChartPoint, string> defaultLabel = x =>
                string.Format("O: {0}, H: {1}, L: {2} C: {3}", x.Open, x.High, x.Low, x.Close);
            this.SetIfNotSet(LabelPointProperty, defaultLabel);

            DefaultFillOpacity = 1;
        }

        #endregion
    }
}
