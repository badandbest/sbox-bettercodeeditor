using System.IO;
using System.Linq;
using Editor;
using Sandbox;
using Sandbox.UI;
using Button = Editor.Button;

namespace BetterCodeEditor;

[Inspector( typeof(AssetEntry) )]
public sealed class CodeInspector : InspectorWidget
{
	[Event( "editor.created" )]
	public static void Init( EditorMainWindow _ ) => AssetBrowser.Get().OnHighlight += entries =>
	{
		var entry = entries.OfType<AssetEntry>().First();
		if ( EditorUtility.IsCodeFile( entry.AbsolutePath ) )
		{
			EditorUtility.InspectorObject = entry;
		}
	};

	public AssetEntry Asset;
	public TextEdit Code;
	public Button SaveBtn;

	public CodeInspector( SerializedObject so ) : base( so )
	{
		Asset = so.Targets.OfType<AssetEntry>().Single();
		Layout = Layout.Column();
		Layout.Margin = new Margin( 0, 38f, 0, 0 );

		var body = Layout.AddColumn();
		{
			body.Margin = 8;
			Code = body.Add( new TextEdit() );
			Code.TextChanged = _ => SaveBtn!.Enabled = true;
		}

		Layout.AddSeparator();

		var footer = Layout.AddRow();
		{
			footer.Spacing = 8;
			footer.Margin = new Margin( 8, 8 );

			footer.AddStretchCell();

			var revert = footer.Add( new Button( "Revert Changes", "history" ) );
			revert.Clicked = Load;

			SaveBtn = footer.Add( new Button( "Save", "save" ) );
			SaveBtn.Clicked = Save;
		}

		Load();
	}

	protected override void OnPaint()
	{
		var rect = LocalRect;
		rect.Top = 6;
		rect.Bottom = 36;
		rect.Left = 12;

		Paint.DrawIcon( rect, "code", 22, TextFlag.LeftCenter );

		rect.Left += 42;

		Paint.SetHeadingFont( 9 );
		Paint.DrawText( rect, Path.GetFileNameWithoutExtension( Asset.Name ), TextFlag.LeftTop );
		Paint.SetDefaultFont();
		Paint.SetPen( Theme.Text.WithAlpha( 0.5f ) );
		Paint.DrawText( rect, Path.GetRelativePath( Project.Current.GetRootPath(), Asset.AbsolutePath ),
			TextFlag.LeftBottom );
	}

	public void Load()
	{
		Code.PlainText = File.ReadAllText( Asset.AbsolutePath );
		SaveBtn.Enabled = false;
	}

	public void Save()
	{
		File.WriteAllText( Asset.AbsolutePath, Code.PlainText );
		SaveBtn.Enabled = false;
	}
}
