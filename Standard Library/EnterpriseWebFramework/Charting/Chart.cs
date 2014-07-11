﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using JetBrains.Annotations;
using RedStapler.StandardLibrary.EnterpriseWebFramework.Controls;
using RedStapler.StandardLibrary.EnterpriseWebFramework.CssHandling;
using RedStapler.StandardLibrary.IO;
using RedStapler.StandardLibrary.WebFileSending;

namespace RedStapler.StandardLibrary.EnterpriseWebFramework {
	/// <summary>
	/// A control capable of displaying chart data.
	/// </summary>
	public class Chart: WebControl, ControlTreeDataLoader {
		internal class CssElementCreator: ControlCssElementCreator {
			internal const string CssClass = "ewfChart";

			CssElement[] ControlCssElementCreator.CreateCssElements() {
				return new[] { new CssElement( "Chart", "div." + CssClass ) };
			}
		}

		/// <summary>
		/// All data in the chart to display.
		/// </summary>
		public class ReportData {
			public readonly int MaxXValues;

			[ NotNull ]
			public readonly string LabelsTitle;

			[ NotNull ]
			public readonly IEnumerable<string> Labels;

			[ NotNull ]
			public readonly IEnumerable<DataSeries> Values;

			[ CanBeNull ]
			public readonly Func<Color> NextColorSelector;

			/// <summary>
			/// The sum of all of the data for the chart.
			/// Assuming <paramref name="values"/> has multiple elements, draws multiple sets of Y values on the same chart.
			/// </summary>
			/// <param name="labelsTitle">The label for the X values.</param>
			/// <param name="labels">The labels for the X axis. There must be exactly as many elements as there are in each of the elements of <paramref name="values"/></param>.
			/// <param name="values">Y values.</param>
			/// <param name="nextColorSelector">When set, returns the next color used to be used for the current <see cref="DataSeries"/></param>.
			/// /// <param name="maxXValues">The amount of values to display on the x axis. This menas only the last <paramref name="maxXValues"/> values are displayed.</param>
			public ReportData(
				string labelsTitle, IEnumerable<string> labels, IEnumerable<DataSeries> values, Func<Color> nextColorSelector = null, int maxXValues = 16 ) {
				LabelsTitle = labelsTitle;
				Labels = labels;
				Values = values;
				NextColorSelector = nextColorSelector;
				MaxXValues = maxXValues;
			}

			/// <summary>
			/// The sum of all of the data for the chart.
			/// </summary>
			/// <param name="labelsTitle">The label for the X values.</param>
			/// <param name="labels">The labels for the X axis. There must be exactly as many elements as there are in <paramref name="values"/></param>.
			/// <param name="values">Y values.</param>
			/// <param name="color">The color to use to represent this data.</param>
			/// <param name="maxXValues">The amount of values to display on the x axis. This menas only the last <paramref name="maxXValues"/> values are displayed.</param>
			public ReportData( string labelsTitle, IEnumerable<string> labels, DataSeries values, Color? color = null, int maxXValues = 16 )
				: this( labelsTitle, labels, values.ToSingleElementArray(), color != null ? () => color.Value : (Func<Color>)null, maxXValues: maxXValues ) {}
		}

		#region Chart.js configuration

		// ReSharper disable All

		/// <summary>
		/// Used for Line graphs.
		/// JSON object used to configure Chart.js.
		/// </summary>
		private class Dataset: BaseDataset {
			public readonly string pointStrokeColor = "#fff";
			public readonly string pointColor;

			public Dataset( Color color, IEnumerable<double> data ): base( color, data ) {
				pointColor = strokeColor;
			}
		}

		/// <summary>
		/// Used for Bar graphs.
		/// JSON object used to configure Chart.js.
		/// </summary>
		private class BaseDataset {
			private static string toRgbaString( Color color, string opacity ) {
				return string.Format( "rgba({0},{1},{2},{3})", color.R, color.G, color.B, opacity );
			}

			public readonly string fillColor;
			public readonly string strokeColor;
			public readonly IEnumerable<double> data;

			public BaseDataset( Color color, IEnumerable<double> data ) {
				fillColor = toRgbaString( color, "0.5" );
				strokeColor = toRgbaString( color, "1" );
				this.data = data;
			}
		}

		/// <summary>
		/// JSON object used to configure Chart.js.
		/// </summary>
		private class ChartData {
			public readonly IEnumerable<string> labels;
			public readonly IEnumerable<BaseDataset> datasets;

			public ChartData( IEnumerable<string> labels, IEnumerable<BaseDataset> datasets ) {
				this.labels = labels;
				this.datasets = datasets;
			}
		}

		private class BarOptions: OptionsBase {
			public bool barShowStroke = true;
			public int barStrokeWidth = 1;
			public int barValueSpacing = 5;
			public int barDatasetSpacing = 1;
		}

		private class LineOptions: OptionsBase {
			public bool bezierCurve = true;
			public bool pointDot = true;
			public int pointDotRadius = 3;
			public int pointDotStrokeWidth = 1;
			public bool datasetStroke = true;
			public int datasetStrokeWidth = 2;
			public bool datasetFill = true;
		}

		private class OptionsBase {
			public bool scaleOverlay = false;
			public bool scaleOverride = false;
			public int? scaleSteps = null;
			public int? scaleStepWidth = null;
			public int? scaleStartValue = null;
			public string scaleLineColor = "rgba(0,0,0,.1)";
			public int scaleLineWidth = 1;
			public bool scaleShowLabels = true;
			//public string scaleLabel = ""; // 'null' breaks it; it needs to be "undefined"
			public string scaleFontFamily = "'Arial'";
			public int scaleFontSize = 12;
			public string scaleFontStyle = "normal";
			public string scaleFontColor = "#666";
			public bool scaleShowGridLines = true;
			public string scaleGridLineColor = "rgba(0,0,0,.05)";
			public int scaleGridLineWidth = 1;
			public bool animation = true;
			public int animationSteps = 60;
			public string animationEasing = "easeOutQuart";
			public string onAnimationComplete = null;
		}

		// ReSharper restore All

		#endregion

		private static Func<Color> getDefaultNextColorSelectors() {
			var c = nextColor().GetEnumerator();
			return () => {
				c.MoveNext();
				return c.Current;
			};
		}

		private static IEnumerable<Color> nextColor() {
			yield return Color.FromArgb( 120, 160, 195 );
			yield return Color.FromArgb( 255, 182, 149 );
			yield return Color.FromArgb( 170, 225, 748 );
			yield return Color.FromArgb( 255, 230, 149 );
			var rand = new Random();
			while( true )
				yield return Color.FromArgb( rand.Next( 256 ), rand.Next( 256 ), rand.Next( 256 ) );
		}

		private readonly ChartType chartType;
		private readonly ReportData reportData;
		private readonly string exportName;

		/// <summary>
		/// Creates a Chart displaying a supported <see cref="ChartType"/> with the given data,
		/// including as a chart, with a table, that allows exporting the data to CSV.
		/// </summary>
		/// <param name="chartType"></param>
		/// <param name="reportData">The data to display</param>
		/// <param name="exportName">Used to create a meaningful file name when exporting the data.</param>
		public Chart( ChartType chartType, ReportData reportData, string exportName ) {
			this.chartType = chartType;
			this.reportData = reportData;
			this.exportName = exportName;
		}

		void ControlTreeDataLoader.LoadData() {
			CssClass = CssClass.ConcatenateWithSpace( CssElementCreator.CssClass );

			var getNextColor = reportData.NextColorSelector ?? getDefaultNextColorSelectors();

			Func<DataSeries, BaseDataset> selector;
			OptionsBase options;
			switch( chartType ) {
				case ChartType.Line:
					selector = v => new Dataset( getNextColor(), v.Values.TakeLast( reportData.MaxXValues ) );
					options = new LineOptions { bezierCurve = false };
					break;
				case ChartType.Bar:
					selector = v => new BaseDataset( getNextColor(), v.Values.TakeLast( reportData.MaxXValues ) );
					options = new BarOptions { };
					break;
				default:
					throw new UnexpectedValueException( chartType );
			}

			var headers = reportData.LabelsTitle.ToSingleElementArray().Concat( reportData.Values.Select( v => v.Title ) );
			var tableData = new List<IEnumerable<string>>( reportData.Values.First().Values.Count() );
			for( var i = 0; i < tableData.Capacity; i++ ) {
				var i1 = i;
				tableData.Add( reportData.Labels.ElementAt( i1 ).ToSingleElementArray().Concat( reportData.Values.Select( v => v.Values.ElementAt( i1 ).ToString() ) ) );
			}

			var chartData = new ChartData( reportData.Labels.TakeLast( reportData.MaxXValues ), reportData.Values.Select( selector ).ToArray() );

			Controls.Add( getExportButton( headers, tableData ) );

			var chartClientId = ClientID + "Chart";
			switch( chartType ) {
				case ChartType.Bar:
				case ChartType.Line:
					Controls.Add( new Literal { Text = "<canvas id='{0}' height='400'></canvas>".FormatWith( chartClientId ) } );
					break;
				default:
					throw new UnexpectedValueException( chartType );
			}

			if( chartData.datasets.Count() > 1 ) {
				Controls.Add(
					new Box(
						"Key",
						new ControlLine(
							chartData.datasets.Select(
								( dataset, i ) =>
								new Literal
									{
										Text =
											@"<div style='display: inline-block; vertical-align: middle; width: 20px; height: 20px; background-color: {0}; border: 1px solid {1};'>&nbsp;</div> {2}"
									.FormatWith( dataset.fillColor, dataset.strokeColor, reportData.Values.ElementAt( i ).Title )
									} ).ToArray() ).ToSingleElementArray() ) );
			}


			var table = EwfTable.CreateWithItems(
				defaultItemLimit: DataRowLimit.Unlimited,
				headItems: new[] { new EwfTableItem( headers.Select( c => new EwfTableCell( c ) ) ) } );
			table.AddData( tableData, cs => new EwfTableItem( cs.Select( c => new EwfTableCell( c ) ) ) );
			Controls.Add( table );

			Controls.Add( new Literal { Text = @"
<script type=""text/javascript"">
    var chart = document.getElementById( ""{3}"" );
	chart.width = $( chart ).parent().width();
    var ctx = chart.getContext( ""2d"" );
    var data = {0};
    var options = {1};
    new Chart( ctx ).{2}( data, options );
</script>
".FormatWith( chartData.ToJson(), options.ToJson(), chartType, chartClientId ) } );
		}

		private Block getExportButton( IEnumerable<string> headers, List<IEnumerable<string>> tableData ) {
			var block = new Block(
				new PostBackButton(
					PostBack.CreateFull(
						id: ClientID + "Export",
						actionGetter: () => new PostBackAction(
							                    new FileCreator(
							                    output => {
								                    var csv = new CsvFileWriter();
								                    var writer = new StreamWriter( output );

								                    csv.AddValuesToLine( headers.ToArray() );
								                    csv.WriteCurrentLineToFile( writer );
								                    foreach( var td in tableData ) {
									                    csv.AddValuesToLine( td.ToArray() );
									                    csv.WriteCurrentLineToFile( writer );
								                    }

								                    return new FileInfoToBeSent( "{0} {1}.csv".FormatWith( exportName, DateTime.Now ), "text/csv" );
							                    } ) ) ),
					new TextActionControlStyle( "Export" ),
					usesSubmitBehavior: false ) );
			block.Style.Add( "text-align", "right" );
			return block;
		}

		/// <summary>
		/// Returns the tag that represents this control in HTML.
		/// </summary>
		protected override HtmlTextWriterTag TagKey { get { return HtmlTextWriterTag.Div; } }
	}
}