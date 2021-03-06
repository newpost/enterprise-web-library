using System.Web.UI;
using System.Web.UI.WebControls;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;

namespace EnterpriseWebLibrary.WebSite.TestPages {
	partial class ActionControls: EwfPage {
		protected override void loadData() {
			ph.AddControlsReturnThis(
				getBox(
					new PostBackButton(
						new ButtonActionControlStyle( "Tiny Post Back Button", buttonSize: ButtonSize.ShrinkWrap ),
						usesSubmitBehavior: false,
						postBack: PostBack.CreateFull( id: "tiny" ) ) ) );
			ph.AddControlsReturnThis(
				getBox( EwfLink.Create( SubFolder.General.GetInfo(), new ButtonActionControlStyle( "Tiny EWF Link", buttonSize: ButtonSize.ShrinkWrap ) ) ) );
			ph.AddControlsReturnThis(
				getBox(
					new ToggleButton(
						new WebControl[ 0 ],
						new ButtonActionControlStyle( "Tiny Toggle Button", buttonSize: ButtonSize.ShrinkWrap ),
						false,
						( postBackValue, validator ) => {} ) ) );

			ph.AddControlsReturnThis(
				getBox(
					new PostBackButton( new ButtonActionControlStyle( "Post Back Button" ), usesSubmitBehavior: false, postBack: PostBack.CreateFull( id: "normal" ) )
						{
							Width = Unit.Pixel( 200 )
						} ) );
			ph.AddControlsReturnThis( getBox( EwfLink.Create( EwfTableDemo.GetInfo(), new ButtonActionControlStyle( "EWF Link" ) ) ) );
			ph.AddControlsReturnThis(
				getBox( new ToggleButton( new WebControl[ 0 ], new ButtonActionControlStyle( "Toggle button" ), false, ( postBackValue, validator ) => {} ) ) );

			ph.AddControlsReturnThis(
				getBox(
					new PostBackButton(
						new ButtonActionControlStyle( "Large Post Back Button", buttonSize: ButtonSize.Large ),
						usesSubmitBehavior: false,
						postBack: PostBack.CreateFull( id: "large" ) ) ) );
			ph.AddControlsReturnThis( getBox( EwfLink.Create( EwfTableDemo.GetInfo(), new ButtonActionControlStyle( "Large EWF Link", ButtonSize.Large ) ) ) );
			ph.AddControlsReturnThis(
				getBox(
					new ToggleButton(
						new WebControl[ 0 ],
						new ButtonActionControlStyle( "Large Toggle Button", ButtonSize.Large ),
						false,
						( postBackValue, validator ) => {} ) ) );
		}

		private Control getBox( Control contentControl ) {
			return new LegacySection( contentControl.ToCollection(), style: SectionStyle.Box );
		}
	}
}