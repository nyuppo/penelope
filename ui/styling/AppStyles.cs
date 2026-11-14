#nullable enable

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Styling;

namespace penelope;

public static class AppStyles
{
    private static DynamicResourceExtension R(string key) => new(key);

    public static Styles Create()
    {
        var styles = new Styles();

        AddResources(styles);
        AddBase(styles);
        AddText(styles);
        AddSurfaces(styles);
        AddButtons(styles);
        AddTree(styles);
        AddInputs(styles);
        AddMenu(styles);

        return styles;
    }

    private static void AddResources(Styles styles)
    {
        var r = styles.Resources;

        // COLOURS
        r["Colour_Linen"] = new SolidColorBrush(Color.Parse("#EBE3D0"));
        r["Colour_Thread"] = new SolidColorBrush(Color.Parse("#A8483A"));
        r["Colour_Moss"] = new SolidColorBrush(Color.Parse("#3D5F38"));

        r["Colour_Paper_50"] = new SolidColorBrush(Color.Parse("#FCFAF4"));
        r["Colour_Paper_100"] = new SolidColorBrush(Color.Parse("#F6F1E5"));
        r["Colour_Paper_200"] = new SolidColorBrush(Color.Parse("#EDE6D6"));
        r["Colour_Paper_300"] = new SolidColorBrush(Color.Parse("#E2D9C4"));

        r["Colour_Deco_Rule_Blue"] = new SolidColorBrush(Color.Parse("#CFDBE7"));
        r["Colour_Deco_Margin_Red"] = new SolidColorBrush(Color.Parse("#E3A59A"));

        r["Colour_Ink"] = new SolidColorBrush(Color.Parse("#2B2620"));
        r["Colour_Ink_Soft"] = new SolidColorBrush(Color.Parse("#5E574B"));
        r["Colour_Ink_Faint"] = new SolidColorBrush(Color.Parse("#8C8373"));

        r["Colour_Fountain"] = new SolidColorBrush(Color.Parse("#2F4A7A"));
        r["Colour_Fountain_Deep"] = new SolidColorBrush(Color.Parse("#22385F"));
        r["Colour_Fountain_Wash"] = new SolidColorBrush(Color.Parse("#E3E9F2"));

        r["Colour_Highlighter"] = new SolidColorBrush(Color.Parse("#F7E38C"));
        r["Colour_Highlighter_Edge"] = new SolidColorBrush(Color.Parse("#E7C94F"));

        r["Colour_Note_Yellow"] = new SolidColorBrush(Color.Parse("#FBEAA6"));
        r["Colour_Note_Pink"] = new SolidColorBrush(Color.Parse("#F7D3C8"));
        r["Colour_Note_Green"] = new SolidColorBrush(Color.Parse("#D9E7C9"));
        r["Colour_Note_Blue"] = new SolidColorBrush(Color.Parse("#D3E2EF"));

        r["Colour_Tab_Hover"] = new SolidColorBrush(Color.Parse("#73FFFFFF"));

        // SHADOWS
        r["Shadow_None"] = new BoxShadows();
        r["Shadow_Paper"] = BoxShadows.Parse("inset 0 1 0 0 #B3FFFFFF, 0 1 2 0 #14463419");
        r["Shadow_Sheet"] = BoxShadows.Parse("0 1 1 0 #0F463419, 0 8 20 -8 #38463419");
        r["Shadow_Lift"] = BoxShadows.Parse("0 2 3 0 #1A463419, 0 14 24 -10 #4D463419");
        r["Shadow_Pressed"] = BoxShadows.Parse("inset 0 1 2 0 #24463419");
        r["Shadow_Primary"] = BoxShadows.Parse("inset 0 1 0 0 #2EFFFFFF, 0 1 2 0 #4022385F");

        // SPACING
        r["Space_1"] = 4.0;
        r["Space_2"] = 8.0;
        r["Space_3"] = 12.0;
        r["Space_4"] = 16.0;
        r["Space_6"] = 24.0;
        r["Space_8"] = 32.0;

        // CORNERS
        r["Radius_Sm"] = new CornerRadius(3);
        r["Radius_Md"] = new CornerRadius(6);
        r["Radius_Lg"] = new CornerRadius(10);

        // TYPOGRAPHY
        r["Font_Display"] = new FontFamily("avares://penelope/Assets/Fonts/Fraunces#Fraunces 72pt");
        r["Font_Interface"] = new FontFamily("avares://penelope/Assets/Fonts/InstrumentSans#Instrument Sans");
        r["Font_Hand"] = new FontFamily("avares://penelope/Assets/Fonts/Caveat#Caveat");

        // TYPOGRAPHY - DISPLAY
        r["FontSize_Display"] = 32.0;
        r["FontSize_Title"] = 20.0;

        // TYPOGRAPHY - INTERFACE
        r["FontSize_Heading"] = 15.0;
        r["FontSize_Body"] = 14.0;
        r["FontSize_Label"] = 13.0;
        r["FontSize_Caption"] = 12.0;
        r["FontSize_Overline"] = 11.0;

        // TYPOGRAPHY - ACCENT
        r["FontSize_Hand"] = 20.0;
        r["FontSize_Hand_Small"] = 14.0;

        // LINE HEIGHTS
        r["LineHeight_Display"] = 38.0;
        r["LineHeight_Title"] = 26.0;
        r["LineHeight_Heading"] = 20.0;
        r["LineHeight_Body"] = 20.0;
        r["LineHeight_Label"] = 18.0;
        r["LineHeight_Caption"] = 16.0;
        r["LineHeight_Overline"] = 14.0;
        r["LineHeight_Hand"] = 24.0;
        r["LineHeight_Hand_Small"] = 16.0;
    }

    private static void AddBase(Styles styles)
    {
        // defaults on Window
        styles.Add(new Style(x => x.Is<Window>()) {
            Setters = {
                new Setter(TemplatedControl.BackgroundProperty, R("Colour_Paper_100")),
                new Setter(TemplatedControl.ForegroundProperty, R("Colour_Ink")),
                new Setter(TemplatedControl.FontFamilyProperty, R("Font_Interface")),
                new Setter(TemplatedControl.FontSizeProperty, R("FontSize_Body")),
            }
        });

        // highlter text selection
        styles.Add(new Style(x => x.Is<TextBox>())
        {
            Setters =
            {
                new Setter(TextBox.SelectionBrushProperty, R("Colour_Highlighter")),
                new Setter(TextBox.SelectionForegroundBrushProperty, R("Colour_Ink")),
            }
        });
        styles.Add(new Style(x => x.Is<SelectableTextBlock>())
        {
            Setters =
            {
                new Setter(SelectableTextBlock.SelectionBrushProperty, R("Colour_Highlighter")),
            }
        });

        // icons (PNGs)
        styles.Add(new Style(x => x.OfType<Border>().Class("glyph"))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, R("Colour_Ink_Soft")),
                new Setter(Layoutable.WidthProperty, 16.0),
                new Setter(Layoutable.HeightProperty, 16.0),
                new Setter(Layoutable.VerticalAlignmentProperty, Avalonia.Layout.VerticalAlignment.Center),
                new Setter(Layoutable.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Center),
            }
        });
        styles.Add(new Style(x => x.OfType<Border>().Class("glyph").Class("chevron"))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, R("Colour_Ink_Faint")),
                new Setter(Layoutable.WidthProperty, 12.0),
                new Setter(Layoutable.HeightProperty, 12.0),
            }
        });

        // temp icon standins
        styles.Add(new Style(x => x.OfType<Border>().Class("glyph").Class("placeholder"))
        {
            Setters =
            {
                new Setter(Visual.OpacityProperty, 0.4),
                new Setter(Border.CornerRadiusProperty, new CornerRadius(4)),
            }
        });

        // focus ring
        styles.Add(new Style(x => x.OfType<Border>().Class("focus-ring"))
        {
            Setters =
            {
                new Setter(Border.BorderBrushProperty, R("Colour_Fountain")),
                new Setter(Border.BorderThicknessProperty, new Thickness(2)),
                new Setter(InputElement.IsHitTestVisibleProperty, false),
            }
        });
    }

    private static Style TextStyle(string cls, string font, string size, string lineHeight, FontWeight weight) =>
        new(x => x.OfType<TextBlock>().Class(cls))
        {
            Setters =
            {
                new Setter(TextBlock.FontFamilyProperty, R(font)),
                new Setter(TextBlock.FontSizeProperty, R(size)),
                new Setter(TextBlock.LineHeightProperty, R(lineHeight)),
                new Setter(TextBlock.FontWeightProperty, weight),
            }
        };

    private static FuncTemplate<Control> FocusRing(double offset, double baseRadius) => new(() => new Border
        {
            Classes = { "focus-ring" },
            Margin = new Thickness(-(offset + 2)),
            CornerRadius = new CornerRadius(baseRadius + offset + 2),
        }
    );

    private static void AddText(Styles styles)
    {
        styles.Add(TextStyle("display", "Font_Display", "FontSize_Display", "LineHeight_Display", FontWeight.SemiBold));
        styles.Add(TextStyle("title", "Font_Display", "FontSize_Title", "LineHeight_Title", FontWeight.SemiBold));
        styles.Add(TextStyle("heading", "Font_Interface", "FontSize_Heading", "LineHeight_Heading", FontWeight.SemiBold));
        styles.Add(TextStyle("body", "Font_Interface", "FontSize_Body", "LineHeight_Body", FontWeight.Normal));
        styles.Add(TextStyle("label", "Font_Interface", "FontSize_Label", "LineHeight_Label", FontWeight.Medium));
        styles.Add(TextStyle("caption", "Font_Interface", "FontSize_Caption", "LineHeight_Caption", FontWeight.Normal));
        styles.Add(TextStyle("overline", "Font_Interface", "FontSize_Overline", "LineHeight_Overline", FontWeight.SemiBold));
        styles.Add(TextStyle("mono", "Font_Mono", "FontSize_Mono", "LineHeight_Mono", FontWeight.Normal));
        styles.Add(TextStyle("hand", "Font_Hand", "FontSize_Hand", "LineHeight_Hand", FontWeight.Medium));
        styles.Add(TextStyle("hand_small", "Font_Hand", "FontSize_Hand_Small", "LineHeight_Hand_Small", FontWeight.Medium));


        styles.Add(new Style(x => x.OfType<TextBlock>().Class("overline"))
        {
            Setters =
            {
                new Setter(TextBlock.LetterSpacingProperty, 0.9),
                new Setter(TextBlock.ForegroundProperty, R("Colour_Ink_Soft")),
            }
        });

        styles.Add(new Style(x => x.OfType<TextBlock>().Class("caption"))
        {
            Setters = { new Setter(TextBlock.ForegroundProperty, R("Colour_Ink_Soft")) }
        });
        styles.Add(new Style(x => x.OfType<TextBlock>().Class("mono"))
        {
            Setters = { new Setter(TextBlock.ForegroundProperty, R("Colour_Ink_Soft")) }
        });

        styles.Add(new Style(x => x.OfType<TextBlock>().Class("soft"))
        {
            Setters = { new Setter(TextBlock.ForegroundProperty, R("Colour_Ink_Soft")) }
        });
        styles.Add(new Style(x => x.OfType<TextBlock>().Class("faint"))
        {
            Setters = { new Setter(TextBlock.ForegroundProperty, R("Colour_Ink_Faint")) }
        });
        styles.Add(new Style(x => x.OfType<TextBlock>().Class("fountain"))
        {
            Setters = { new Setter(TextBlock.ForegroundProperty, R("Colour_Fountain")) }
        });
        styles.Add(new Style(x => x.OfType<TextBlock>().Class("display"))
        {
            Setters = { new Setter(TextBlock.LetterSpacingProperty, -0.32) }
        });
    }

    private static void AddSurfaces(Styles styles)
    {
        // desk: app ground, sidebar
        styles.Add(new Style(x => x.OfType<Border>().Class("desk"))
        {
            Setters = { new Setter(Border.BackgroundProperty, R("Colour_Paper_100")) }
        });

        // sheet: viewer page, documents, etc
        // can be layered with ruled lines to turn into notebook paper
        styles.Add(new Style(x => x.OfType<Border>().Class("sheet"))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, R("Colour_Paper_50")),
                new Setter(Border.CornerRadiusProperty, R("Radius_Sm")),
                new Setter(Border.BoxShadowProperty, R("Shadow_Sheet")),
                new Setter(Border.ClipToBoundsProperty, true),
            }
        });

        // card: popovers, panels
        styles.Add(new Style(x => x.OfType<Border>().Class("card"))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, R("Colour_Paper_50")),
                new Setter(Border.BorderBrushProperty, R("Colour_Paper_300")),
                new Setter(Border.BorderThicknessProperty, new Thickness(1)),
                new Setter(Border.CornerRadiusProperty, R("Radius_Lg")),
                new Setter(Border.BoxShadowProperty, R("Shadow_Lift")),
                new Setter(Decorator.PaddingProperty, new Thickness(12)),
            }
        });

        styles.Add(new Style(x => x.OfType<Border>().Class("viewer-header"))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, R("Colour_Paper_50")),
                new Setter(Border.BorderBrushProperty, R("Colour_Paper_300")),
                new Setter(Border.BorderThicknessProperty, new Thickness(0, 0, 0, 1)),
                new Setter(Border.BoxShadowProperty, R("Shadow_Sheet")),
                new Setter(Decorator.PaddingProperty, new Thickness(12, 8)),
            }
        });
        styles.Add(new Style(x => x.OfType<Border>().Class("file-chip"))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, R("Colour_Fountain_Wash")),
                new Setter(Border.CornerRadiusProperty, R("Radius_Md")),
                new Setter(Layoutable.WidthProperty, 32.0),
                new Setter(Layoutable.HeightProperty, 32.0),
                new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Center),
            }
        });
        styles.Add(new Style(x => x.OfType<Border>().Class("header-actions"))
        {
            Setters =
            {
                new Setter(Border.BorderBrushProperty, R("Colour_Paper_300")),
                new Setter(Border.BorderThicknessProperty, new Thickness(1, 0, 0, 0)),
                new Setter(Decorator.PaddingProperty, new Thickness(8, 0, 0, 0)),
                new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Center),
            }
        });
        styles.Add(new Style(x => x.OfType<Border>().Class("glyph").Class("fountain"))
        {
            Setters = { new Setter(Border.BackgroundProperty, R("Colour_Fountain")) }
        });

        // nav: self-explanatory :P
        // place a line stitch along the bottom (see next)
        styles.Add(new Style(x => x.OfType<Border>().Class("nav"))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, R("Colour_Linen")),
                new Setter(Border.BorderBrushProperty, R("Colour_Paper_300")),
                new Setter(Border.BorderThicknessProperty, new Thickness(0, 0, 0, 1)),
                new Setter(Layoutable.HeightProperty, 32.0),
            }
        });

        // stitch: dashed line, 4px above bottom of parent
        styles.Add(new Style(x => x.OfType<Line>().Class("stitch"))
        {
            Setters =
            {
                new Setter(Shape.StrokeProperty, R("Colour_Paper_300")),
                new Setter(Shape.StrokeThicknessProperty, 1.5),
                new Setter(Shape.StrokeDashArrayProperty, new AvaloniaList<double> { 3, 2 }),
                new Setter(Layoutable.VerticalAlignmentProperty, Avalonia.Layout.VerticalAlignment.Bottom),
                new Setter(Layoutable.MarginProperty, new Thickness(0, 0, 0, 4)),
                new Setter(InputElement.IsHitTestVisibleProperty, false),
            }
        });

        // filebar: viewer topbar
        styles.Add(new Style(x => x.OfType<Border>().Class("filebar"))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, R("Colour_Paper_50")),
                new Setter(Border.BorderBrushProperty, R("Colour_Paper_300")),
                new Setter(Border.BorderThicknessProperty, new Thickness(0, 0, 0, 1)),
                new Setter(Layoutable.MinHeightProperty, 56.0),
                new Setter(Decorator.PaddingProperty, new Thickness(16, 0, 12, 0)),
            }
        });

        // dividers
        styles.Add(new Style(x => x.OfType<Border>().Class("divider"))
        {
            Setters = {
                new Setter(Border.BackgroundProperty, R("Colour_Paper_300")),
                new Setter(Layoutable.MinWidthProperty, 1.0),
            }
        });
        styles.Add(new Style(x => x.OfType<GridSplitter>().Class("divider"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, R("Colour_Paper_300")),
                new Setter(Layoutable.MinWidthProperty, 1.0),
            }
        });

        // tree: sidebar container
        styles.Add(new Style(x => x.OfType<Border>().Class("tree"))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, R("Colour_Paper_100")),
                new Setter(Decorator.PaddingProperty, new Thickness(8, 0, 0, 0)),
            }
        });

        styles.Add(new Style(x => x.OfType<DockPanel>().Class("tree-label"))
        {
            Setters = { new Setter(Layoutable.MarginProperty, new Thickness(8, 8, 8, 4)) }
        });
    }

    private static Selector Presenter(Selector s) => s.Template().OfType<ContentPresenter>().Name("PART_ContentPresenter");

    private static void AddButtons(Styles styles)
    {
        // paper button (default)
        styles.Add(new Style(x => x.OfType<Button>())
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, R("Colour_Paper_50")),
                new Setter(TemplatedControl.ForegroundProperty, R("Colour_Ink")),
                new Setter(TemplatedControl.BorderBrushProperty, R("Colour_Paper_300")),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1)),
                new Setter(TemplatedControl.CornerRadiusProperty, R("Radius_Md")),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(12, 0)),
                new Setter(TemplatedControl.FontSizeProperty, R("FontSize_Label")),
                new Setter(TemplatedControl.FontWeightProperty, FontWeight.Medium),
                new Setter(Layoutable.HeightProperty, 30.0),
                new Setter(ContentControl.VerticalContentAlignmentProperty, Avalonia.Layout.VerticalAlignment.Center),
                new Setter(Control.FocusAdornerProperty, FocusRing(2, 6)),
            }
        });
        styles.Add(new Style(x => Presenter(x.OfType<Button>()))
        {
            Setters = { new Setter(ContentPresenter.BoxShadowProperty, R("Shadow_Paper")) }
        });
        styles.Add(new Style(x => Presenter(x.OfType<Button>().Class(":pointerover")))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, R("Colour_Paper_200")),
                new Setter(ContentPresenter.BorderBrushProperty, R("Colour_Paper_300")),
                new Setter(ContentPresenter.ForegroundProperty, R("Colour_Ink")),
            }
        });
        styles.Add(new Style(x => Presenter(x.OfType<Button>().Class(":pressed")))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, R("Colour_Paper_200")),
                new Setter(ContentPresenter.BoxShadowProperty, R("Shadow_Pressed")),
            }
        });
        styles.Add(new Style(x => Presenter(x.OfType<Button>().Class(":disabled")))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, R("Colour_Paper_100")),
                new Setter(ContentPresenter.ForegroundProperty, R("Colour_Ink_Faint")),
                new Setter(ContentPresenter.BoxShadowProperty, R("Shadow_None")),
            }
        });

        // primary button
        styles.Add(new Style(x => x.OfType<Button>().Class("primary"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, R("Colour_Fountain")),
                new Setter(TemplatedControl.ForegroundProperty, R("Colour_Paper_50")),
                new Setter(TemplatedControl.BorderBrushProperty, R("Colour_Fountain_Deep")),
            }
        });
        styles.Add(new Style(x => Presenter(x.OfType<Button>().Class("primary")))
        {
            Setters = { new Setter(ContentPresenter.BoxShadowProperty, R("Shadow_Primary")) }
        });
        styles.Add(new Style(x => Presenter(x.OfType<Button>().Class("primary").Class(":pointerover")))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, R("Colour_Fountain_Deep")),
                new Setter(ContentPresenter.BorderBrushProperty, R("Colour_Fountain_Deep")),
                new Setter(ContentPresenter.ForegroundProperty, R("Colour_Paper_50")),
            }
        });
        styles.Add(new Style(x => Presenter(x.OfType<Button>().Class("primary").Class(":pressed")))
        {
            Setters = {
                new Setter(ContentPresenter.BackgroundProperty, R("Colour_Fountain_Deep")),
                new Setter(ContentPresenter.BoxShadowProperty, R("Shadow_Pressed")),
            }
        });

        // quiet (similar to 'ghost')
        // icon defaults to 30x30
        foreach (var cls in new[] { "quiet", "icon", "tab" })
        {
            styles.Add(new Style(x => x.OfType<Button>().Class(cls))
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                    new Setter(TemplatedControl.BorderBrushProperty, Brushes.Transparent),
                    new Setter(TemplatedControl.ForegroundProperty, R("Colour_Ink_Soft")),
                }
            });
            styles.Add(new Style(x => Presenter(x.OfType<Button>().Class(cls)))
            {
                Setters = { new Setter(ContentPresenter.BoxShadowProperty, R("Shadow_None")) }
            });
            styles.Add(new Style(x => Presenter(x.OfType<Button>().Class(cls).Class(":pointerover")))
            {
                Setters =
                {
                    new Setter(ContentPresenter.BackgroundProperty, R("Colour_Paper_200")),
                    new Setter(ContentPresenter.BorderBrushProperty, Brushes.Transparent),
                    new Setter(ContentPresenter.ForegroundProperty, R("Colour_Ink")),
                }
            });
        }
        styles.Add(new Style(x => x.OfType<Button>().Class("icon"))
        {
            Setters =
            {
                new Setter(Layoutable.WidthProperty, 30.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(0)),
                new Setter(ContentControl.HorizontalContentAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Center),
                new Setter(Control.FocusAdornerProperty, FocusRing(1, 6)),
            }
        });

        // nav tab: quiet, raised paper tab when active (toggle with `button.Classes.Set("active", isCurrent)`)
        styles.Add(new Style(x => x.OfType<Button>().Class("tab"))
        {
            Setters = {
                new Setter(Layoutable.HeightProperty, 12.0)
            }
        });
        styles.Add(new Style(x => Presenter(x.OfType<Button>().Class("tab").Class(":pointerover")))
        {
            Setters = { new Setter(ContentPresenter.BackgroundProperty, R("Colour_Tab_Hover")) }
        });
        styles.Add(new Style(x => x.OfType<Button>().Class("tab").Class("active"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, R("Colour_Paper_50")),
                new Setter(TemplatedControl.ForegroundProperty, R("Colour_Fountain")),
            }
        });
        styles.Add(new Style(x => Presenter(x.OfType<Button>().Class("tab").Class("active")))
        {
            Setters = { new Setter(ContentPresenter.BoxShadowProperty, R("Shadow_Paper")) }
        });
        styles.Add(new Style(x => Presenter(x.OfType<Button>().Class("tab").Class("active").Class(":pointerover")))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, R("Colour_Paper_50")),
                new Setter(ContentPresenter.ForegroundProperty, R("Colour_Fountain")),
            }
        });
    }

    private static void AddTree(Styles styles)
    {
        // expected structure:
        // Border.Row (Classes.Set("selected"/"expanded", ...))
        //   Panel
        //    Border.highlighter (only visible when selected)
        //    Grid (12, 16, *, Auto): Border.glyph.chevron | Border.glyph | TextBlock.body.name | TextBlock.meta
        //
        // indent each level with row Padding.Left = 8 + depth * 16

        styles.Add(new Style(x => x.OfType<Border>().Class("row"))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, Brushes.Transparent),
                new Setter(Border.CornerRadiusProperty, R("Radius_Sm")),
                new Setter(Layoutable.HeightProperty, 28.0),
                new Setter(Decorator.PaddingProperty, new Thickness(8, 0)),
                new Setter(InputElement.CursorProperty, new Cursor(StandardCursorType.Hand)),
                new Setter(InputElement.FocusableProperty, true),
                new Setter(Control.FocusAdornerProperty, FocusRing(-2, 3)),
            }
        });
        styles.Add(new Style(x => x.OfType<Border>().Class("row").Class(":pointerover"))
        {
            Setters = { new Setter(Border.BackgroundProperty, R("Colour_Paper_200")) }
        });
        styles.Add(new Style(x => x.OfType<Border>().Class("row").Class("selected"))
        {
            Setters = { new Setter(Border.BackgroundProperty, Brushes.Transparent) }
        });
 
        styles.Add(new Style(x => x.OfType<Border>().Class("row").Class("expanded").Descendant().OfType<Border>().Class("glyph").Class("chevron"))
        {
            Setters = { new Setter(Visual.RenderTransformProperty, TransformOperations.Parse("rotate(90deg)")) }
        });
 
        styles.Add(new Style(x => x.OfType<Border>().Class("highlighter"))
        {
            Setters =
            {
                new Setter(Visual.IsVisibleProperty, false),
                new Setter(Border.BackgroundProperty, R("Colour_Highlighter")),
                new Setter(Border.BorderBrushProperty, R("Colour_Highlighter_Edge")),
                new Setter(Border.BorderThicknessProperty, new Thickness(3, 0, 0, 0)),
                new Setter(Border.CornerRadiusProperty, new CornerRadius(2, 7, 3, 6)),
                new Setter(Layoutable.MarginProperty, new Thickness(-4, 3, -6, 3)),
                new Setter(Visual.RenderTransformProperty, TransformOperations.Parse("skewX(-6deg) rotate(-0.4deg)")),
                new Setter(InputElement.IsHitTestVisibleProperty, false),
            }
        });
        styles.Add(new Style(x => x.OfType<Border>().Class("row").Class("selected").Descendant().OfType<Border>().Class("highlighter"))
        {
            Setters = { new Setter(Visual.IsVisibleProperty, true) }
        });
        styles.Add(new Style(x => x.OfType<Border>().Class("row").Class("selected").Descendant().OfType<TextBlock>().Class("name"))
        {
            Setters =
            {
                new Setter(TextBlock.FontWeightProperty, FontWeight.Medium),
                new Setter(TextBlock.ForegroundProperty, R("Colour_Ink")),
            }
        });
        styles.Add(new Style(x => x.OfType<Border>().Class("row").Class("selected").Descendant().OfType<TextBlock>().Class("meta"))
        {
            Setters = { new Setter(TextBlock.ForegroundProperty, R("Colour_Ink")) }
        });
        styles.Add(new Style(x => x.OfType<Border>().Class("row").Class("selected").Descendant().OfType<Border>().Class("glyph").Not(y => y.Class("chevron")))
        {
            Setters = { new Setter(Border.BackgroundProperty, R("Colour_Ink")) }
        });
    }

    private static Selector TextBoxBorder(Selector s) => s.Template().OfType<Border>().Name("PART_BorderElement");

    private static void AddMenu(Styles styles)
    {
        styles.Add(new Style(x => x.OfType<Menu>())
        {
            Setters = { new Setter(Layoutable.HeightProperty, 24.0) }
        });

        styles.Add(new Style(x => x.OfType<Menu>().Child().OfType<MenuItem>())
        {
            Setters =
            {
                new Setter(Layoutable.HeightProperty, 24.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 0)),
                new Setter(TemplatedControl.FontSizeProperty, R("FontSize_Label")),
                new Setter(TemplatedControl.FontWeightProperty, FontWeight.Medium),
                new Setter(TemplatedControl.ForegroundProperty, R("Colour_Ink_Soft")),
                new Setter(TemplatedControl.CornerRadiusProperty, R("Radius_Md")),
            }
        });

        static Selector Root(Selector s) => s.Template().OfType<Border>().Name("PART_LayoutRoot");

        styles.Add(new Style(x => Root(x.OfType<Menu>().Child().OfType<MenuItem>()))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, Brushes.Transparent),
                new Setter(Border.CornerRadiusProperty, R("Radius_Md")),
            }
        });
        foreach (var state in new[] { ":pointerover", ":selected", ":open" })
        {
            styles.Add(new Style(x => Root(x.OfType<Menu>().Child().OfType<MenuItem>().Class(state)))
            {
                Setters = { new Setter(Border.BackgroundProperty, R("Colour_Tab_Hover")) }
            });
            styles.Add(new Style(x => x.OfType<Menu>().Child().OfType<MenuItem>().Class(state))
            {
                Setters = { new Setter(TemplatedControl.ForegroundProperty, R("Colour_Ink")) }
            });
        }
    }

    private static void AddInputs(Styles styles)
    {
        styles.Add(new Style(x => x.OfType<TextBox>().Class("input"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, R("Colour_Paper_50")),
                new Setter(TemplatedControl.BorderBrushProperty, R("Colour_Paper_300")),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1)),
                new Setter(TemplatedControl.CornerRadiusProperty, R("Radius_Md")),
                new Setter(TemplatedControl.FontSizeProperty, R("FontSize_Label")),
                new Setter(Layoutable.MinHeightProperty, 30.0),
                new Setter(Layoutable.HeightProperty, 30.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(10, 5)),
            }
        });
        styles.Add(new Style(x => TextBoxBorder(x.OfType<TextBox>().Class("input")))
        {
            Setters = { new Setter(Border.BoxShadowProperty, R("Shadow_Pressed")) }
        });
        styles.Add(new Style(x => TextBoxBorder(x.OfType<TextBox>().Class("input").Class(":pointerover")))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, R("Colour_Paper_50")),
                new Setter(Border.BorderBrushProperty, R("Colour_Ink_Faint")),
            }
        });
        styles.Add(new Style(x => TextBoxBorder(x.OfType<TextBox>().Class("input").Class(":focus")))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, R("Colour_Paper_50")),
                new Setter(Border.BorderBrushProperty, R("Colour_Fountain")),
                new Setter(Border.BorderThicknessProperty, new Thickness(1)),
            }
        });
    }
}

// mimics notebook with blue lines every 28px and double red margin rule.
// intended to be put first in a Panel inside a Border.sheet, e.g.:
//   new Border { Classes = { "sheet" }, Child = new Panel { Children = { new RuledLines(), content } } }
public class RuledLines : Control
{
    public static readonly StyledProperty<IBrush?> RuleBrushProperty = AvaloniaProperty.Register<RuledLines, IBrush?>(nameof(RuleBrush));
 
    public static readonly StyledProperty<IBrush?> MarginBrushProperty = AvaloniaProperty.Register<RuledLines, IBrush?>(nameof(MarginBrush));
 
    public static readonly StyledProperty<double> SpacingProperty = AvaloniaProperty.Register<RuledLines, double>(nameof(Spacing), 28.0);
 
    public static readonly StyledProperty<double> MarginXProperty = AvaloniaProperty.Register<RuledLines, double>(nameof(MarginX), 56.0);
 
    public IBrush? RuleBrush { get => GetValue(RuleBrushProperty); set => SetValue(RuleBrushProperty, value); }
    public IBrush? MarginBrush { get => GetValue(MarginBrushProperty); set => SetValue(MarginBrushProperty, value); }
    public double Spacing { get => GetValue(SpacingProperty); set => SetValue(SpacingProperty, value); }
    public double MarginX { get => GetValue(MarginXProperty); set => SetValue(MarginXProperty, value); }
 
    static RuledLines()
    {
        AffectsRender<RuledLines>(RuleBrushProperty, MarginBrushProperty, SpacingProperty, MarginXProperty);
    }
 
    public RuledLines()
    {
        IsHitTestVisible = false;
        this[!RuleBrushProperty] = this.GetResourceObservable("Colour_Deco_Rule_Blue").ToBinding();
        this[!MarginBrushProperty] = this.GetResourceObservable("Colour_Deco_Margin_Red").ToBinding();
    }
 
    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
 
        if (RuleBrush is { } rule && Spacing > 1)
        {
            var pen = new Pen(rule, 1);
            for (var y = 12 + Spacing - 0.5; y < h; y += Spacing)
            {
                context.DrawLine(pen, new Point(0, y), new Point(w, y));
            }
        }
 
        if (MarginBrush is { } margin && MarginX > 0)
        {
            var pen = new Pen(margin, 1);
            context.DrawLine(pen, new Point(MarginX - 0.5, 0), new Point(MarginX - 0.5, h));
            context.DrawLine(pen, new Point(MarginX + 3.5, 0), new Point(MarginX + 3.5, h));
        }
    }
}

// mimics woven crosshatch texture with 1px lines every 3px
public class LinenTexture : Control
{
    static readonly Pen Thread = new(new SolidColorBrush(Color.Parse("#0B78643C")), 1);
    public LinenTexture() => IsHitTestVisible = false;
    public override void Render(DrawingContext ctx)
    {
        var (w, h) = (Bounds.Width, Bounds.Height);
        for (var y = 0.5; y < h; y += 3)
        {
            ctx.DrawLine(Thread, new Point(0, y), new Point(w, y));
        }
        for (var x = 0.5; x < w; x += 3)
        {
            ctx.DrawLine(Thread, new Point(x, 0), new Point(x, h));
        }
    }
}