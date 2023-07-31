using System.Linq;

/// <summary>
/// Formats and unformats blocks of Text for Machine Language Learning & Output Purposes.
/// </summary>
public static class TextFormatter {
    public static readonly char[] vowels = new char[] {
        'a', 'A', 'á', 'Á', 'à', 'À', 'â', 'Â', 'ã', 'Ã', 'ä', 'Ä', 'å', 'Å',
        'e', 'E', 'é', 'É', 'è', 'È', 'ê', 'Ê', 'ë', 'Ë',
        'i', 'I', 'í', 'Í', 'ì', 'Ì', 'î', 'Î', 'ï', 'Ï',
        'o', 'O', 'ó', 'Ó', 'ò', 'Ò', 'ô', 'Ô', 'õ', 'Õ', 'ö', 'Ö',
        'u', 'U', 'ú', 'Ú', 'ù', 'Ù', 'û', 'Û', 'ü', 'Ü',
        'h', 'H',
        'æ', 'Æ',
        'œ', 'Œ',
        'ø', 'Ø',
    }; //A/An Rule for English, other vowels for other languages.
    public static readonly char[] spacingPunctuation = new char[] { ':', ';', ',' };
    public static readonly char[] sentencePunctuation = new char[] { '.', '?', '!' };

    public static readonly string[] breakingSentences = new string[] { ". ", "? ", "! " };
    public static readonly string[] nonbreakingSentences = new string[] { "._", "?_", "!_" };

    #region FormattingFunctions
    /// <summary>
    /// Formats a block of text, keeping punctuation separated from words.
    /// </summary>
    /// <param name="text">The text you want to format. At a minimum, leading spaces will appear before punctuation marks.</param>
    /// <param name="nonbreakingSentences">Set to true to replace all . ! and ? in the text with ._ !_ and ?_</param>
    /// <returns></returns>
    public static string FormatTextPunctuation( string text, bool nonbreakingSentences = false ) {
        string formattedText = text;
        //Go through the block of text and analyze punctuation.
        //Try to determine if it's appropriate to modify.
        for (int i = 0; i < formattedText.Length; i++) {
            if (i > 0 && i < formattedText.Length - 1) {
                //Since we're always looking before and after the character in question, limit to i > 0 and < Length - 1 to avoid errors.
                bool numberBefore = char.IsNumber(formattedText[i - 1]), numberAfter = char.IsNumber(formattedText[i + 1]);
                bool spaceBefore = char.IsWhiteSpace(formattedText[i - 1]), spaceAfter = char.IsWhiteSpace(formattedText[i + 1]);

                if (spacingPunctuation.Contains(formattedText[i])) {
                    //Find internal Punctuation.
                    if (formattedText[i] == ',') {
                        if (numberBefore && numberAfter) {
                            //Commas with numbers on either side are probably data, leave them alone.
                        } else if (!numberBefore && !numberAfter) {
                            //Put spaces around the comma otherwise where necessary.
                            if (!spaceAfter) { formattedText = formattedText.Insert(i + 1, " "); }
                            if (!spaceBefore) { formattedText = formattedText.Insert(i, " "); }
                        } else if (numberBefore && !numberAfter) {
                            //e.g. "In 1997, the organization was founded"
                            if (spaceAfter) { formattedText = formattedText.Insert(i, " "); }
                        }
                    } else if (formattedText[i] == ':') {
                        if (numberBefore && numberAfter) {
                            //Colons with numbers on either side are probably a Ratio or Chapter:Verse style reference, leave them alone here.
                        } else if (!numberBefore && !numberAfter) {
                            //Put spaces around the colon otherwise where necessary.
                            if (!spaceAfter) { formattedText = formattedText.Insert(i + 1, " "); }
                            if (!spaceBefore) { formattedText = formattedText.Insert(i, " "); }
                        }
                    } else if (formattedText[i] == ';') {
                        //Put spaces around semicolons where necessary.
                        if (!spaceAfter) { formattedText = formattedText.Insert(i + 1, " "); }
                        if (!spaceBefore) { formattedText = formattedText.Insert(i, " "); }
                    }
                } else if (sentencePunctuation.Contains(formattedText[i])) {
                    //Find Sentence Breaking Punctuation or Ellipses
                    if (!numberBefore && !numberAfter) {
                        if (nonbreakingSentences && spaceAfter) {
                            char[] textChars = formattedText.ToCharArray();
                            textChars[i + 1] = '_';
                            formattedText = new string(textChars);
                        }

                        if (!spaceBefore && formattedText[i - 1] != formattedText[i]) {
                            if (formattedText[i - 1] == '.') {
                                if (i > 1 && char.IsUpper(formattedText[i - 2])) {
                                    //A.C.R.O.N.Y.M or ALLCAPS, leave it alone.
                                } else {
                                    formattedText = formattedText.Insert(i, " ");
                                }
                            }
                        }
                    }
                }
            } else if (i > 0 && i == formattedText.Length - 1) {
                //This cleans up the last bit of the Text Block.
                bool numberBefore = char.IsNumber(formattedText[i - 1]);
                bool spaceBefore = char.IsWhiteSpace(formattedText[i - 1]);
                //Catch ending ellipses [Why would you do that? We'll let you though, since we thought of it.]
                if (sentencePunctuation.Contains(formattedText[i]) || spacingPunctuation.Contains(formattedText[i])) {
                    //Find Sentence Breaking Punctuation or Ellipses                        
                    if (!spaceBefore && formattedText[i - 1] != formattedText[i]) {
                        if (formattedText[i - 1] != '?' || formattedText[i - 1] != '!') {
                            formattedText = formattedText.Insert(i, " ");
                            break;
                        }
                    }
                }
            }
        }
        return formattedText;
    }

    /// <summary>
    /// Removes non-breaking sentence ._ !_ and ?_ patterns from a block of text, as well as removing leading spaces before punctuation.
    /// </summary>
    /// <param name="text">Text you want to revert.</param>
    /// <returns>Text where all underscores following . ! and ? are removed, and where there are no spaces inserted before punctuation where they wouldn't be.</returns>
    public static string UnformatTextPunctuation( string text ) {
        string unFormattedText = text;
        //Go through the block of text and analyze punctuation.
        //Try to determine if it's appropriate to modify.
        for (int i = 0; i < unFormattedText.Length; i++) {
            if (i > 0 && i < unFormattedText.Length - 1) {
                //Since we're always looking before and after the character in question, limit to i > 0 and < Length - 1 to avoid errors.
                bool spaceBefore = char.IsWhiteSpace(unFormattedText[i - 1]), spaceAfter = char.IsWhiteSpace(unFormattedText[i + 1]);
                if (spacingPunctuation.Contains(unFormattedText[i]) || sentencePunctuation.Contains(unFormattedText[i])) {
                    //Find internal Punctuation, remove leading spaces before.
                    if (spaceBefore) { unFormattedText = unFormattedText.Remove(i - 1, 1); }
                }
            } else if (i > 0 && i == unFormattedText.Length - 1) {
                //This cleans up the last bit of the Text Block.
                bool spaceBefore = char.IsWhiteSpace(unFormattedText[i - 1]);
                //Catch ending punctuation
                if (sentencePunctuation.Contains(unFormattedText[i]) || spacingPunctuation.Contains(unFormattedText[i])) {
                    //Find ending punctuation                     
                    if (spaceBefore) { unFormattedText = unFormattedText.Remove(i - 1, 1); }
                }
            }
        }
        for (int i = 0; i < breakingSentences.Length; i++) {
            unFormattedText = unFormattedText.Replace(nonbreakingSentences[i], breakingSentences[i]);
        }
        return unFormattedText;
    }

    /// <summary>
    /// Capitalizes only the first letter of a string of text.
    /// </summary>
    public static string Capitalize1st( string text ) {
        if (string.IsNullOrEmpty(text)) { return text; }
        char[] capitalized = text.ToCharArray();
        capitalized[0] = char.ToUpper(text[0]);
        return new string(capitalized);
    }

    /// <summary>
    /// Checks if a string of text starts with a Vowel.
    /// </summary>        
    public static bool StartingVowel( string text ) {
        return vowels.Contains(text.ToLower()[0]);
    }

    /// <summary>
    /// Replaces spaces with underscores in a string of text.
    /// </summary>
    /// <param name="_text">The string of text containing spaces.</param>
    /// <returns>The string with underscores instead of spaces.</returns>
    public static string SpacesToUnderscores( string _text ) {
        return _text.Replace(" ", "_");
    }

    /// <summary>
    /// Replaces underscores with spaces in a string of text.
    /// </summary>
    /// <param name="_text">The string of text containing underscores.</param>
    /// <returns>The string with spaces instead of underscores.</returns>
    public static string UnderscoresToSpaces( string _text ) {
        return _text.Replace("_", " ");
    }

    /// <summary>
    /// Removes spaces from a string of text.
    /// </summary>
    /// <param name="_text">The string of text with spaces.</param>
    /// <returns>The string of text with spaces removed.</returns>
    public static string RemoveSpaces( string _text ) {
        return _text.Replace(" ", "");
    }

    /// <summary>
    /// Removes underscores from a string of text.
    /// </summary>
    /// <param name="_text">The string of text with underscores.</param>
    /// <returns>The string of text with underscores removed.</returns>
    public static string RemoveUnderscores( string _text ) {
        return _text.Replace("_", "");
    }

    /// <summary>
    /// Puts spaces before Capital Letters in a String. Useful for CamelCase i.e. things "WithNamesLikeThis". Ignores Acronyms/ALLCAPS blocks.
    /// </summary>
    public static string BreakCamelCase( string text ) {
        if (string.IsNullOrEmpty(text)) {
            return text;
        } else {
            text = text.Trim();
            for (int i = 0; i < text.Length; i++) {
                if (i > 0 && i < text.Length - 1) {
                    if (char.IsUpper(text[i]) && !char.IsUpper(text[i - 1]) && !char.IsWhiteSpace(text[i - 1])) {
                        text = text.Insert(i, " ");
                        i++;
                    }
                }
            }
        }
        return text;
    }
    #endregion
}
