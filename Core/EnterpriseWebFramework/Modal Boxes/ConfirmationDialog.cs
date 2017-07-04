﻿using System.Collections.Generic;
using System.Linq;
using Humanizer;
using MoreLinq;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	/// <summary>
	/// A confirmation dialog box.
	/// </summary>
	public class ConfirmationDialog: EtherealComponent {
		private readonly IReadOnlyCollection<EtherealComponent> children;

		/// <summary>
		/// Creates a confirmation dialog.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="content"></param>
		/// <param name="postBack">Pass null to use the post-back corresponding to the first of the current data modifications.</param>
		public ConfirmationDialog( ConfirmationDialogId id, IEnumerable<FlowComponent> content, PostBack postBack = null ) {
			children =
				new ModalBox(
					id.ModalBoxId,
					content.Concat(
						new Paragraph(
							new EwfButton(
								new StandardButtonStyle( "Cancel" ),
								behavior: new CustomButtonBehavior( () => "document.getElementById( '{0}' ).close();".FormatWith( id.ModalBoxId.ElementId.Id ) ) ).Concat(
									" ".ToComponents() ).Concat( new EwfButton( new StandardButtonStyle( "Continue" ), behavior: new PostBackBehavior( postBack: postBack ) ) ) )
							.ToCollection() ) ).ToCollection();
		}

		IEnumerable<EtherealComponentOrElement> EtherealComponent.GetChildren() {
			return children;
		}
	}
}