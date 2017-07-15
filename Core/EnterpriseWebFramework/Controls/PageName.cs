﻿using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EnterpriseWebLibrary.EnterpriseWebFramework.Controls {
	/// <summary>
	/// A first-level heading that displays the page's name.
	/// </summary>
	public class PageName: WebControl, ControlTreeDataLoader {
		/// <summary>
		/// Gets or sets whether the page name will be excluded if an entity setup exists.
		/// </summary>
		public bool ExcludesPageNameIfEntitySetupExists { get; set; }

		/// <summary>
		/// Returns true if this control will not display any content.
		/// </summary>
		public bool IsEmpty => !getPageName().Any();

		void ControlTreeDataLoader.LoadData() {
			Controls.Add( new PlaceHolder().AddControlsReturnThis( getPageName().ToComponents().GetControls() ) );
		}

		private string getPageName() {
			var es = EwfPage.Instance.EsAsBaseType;
			var info = EwfPage.Instance.InfoAsBaseType;
			return ExcludesPageNameIfEntitySetupExists && es != null && info.ParentResource == null ? es.InfoAsBaseType.EntitySetupName : info.ResourceFullName;
		}

		/// <summary>
		/// Returns the h1 tag, which represents this control in HTML.
		/// </summary>
		protected override HtmlTextWriterTag TagKey => HtmlTextWriterTag.H1;
	}
}