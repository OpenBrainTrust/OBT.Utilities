using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_STANDALONE || UNITY_EDITOR
using UnityEngine;
#endif

/// <summary>
///  Tool for mass keyword searching all strings in any class of object or type. 
///  KeywordSearch takes arrays of generic C# objects, searches for the desired input text, and returns any result records and their text as part of its outputs, ordered by the best match or incidence of partial matches to search words and phrases. 
///  The input object arrays you want to search through can be of mixed types. KeywordSearch can return the strings from both class and array contents, but does not recursively search on its own beyond the first layer - you can however use its object returns themselves to conduct deeper searches.
///  Uses System.Reflection, so be mindful of its accessibility when using it with sensitive embedded information. 
///  Regardless, it will not return values that are compiled as private or similar.
/// </summary>
#if UNITY_STANDALONE || UNITY_EDITOR
public class KeyWordSearch : MonoBehaviour {
[SerializeField] public List<object> objectsToSearch = new List<object>();
#elif !UNITY_STANDALONE && !UNITY_EDITOR
public class KeyWordSearch {
    public List<object> objectsToSearch = new List<object>();
#endif

    /// <summary>
    /// Searches within an array of generic C# objects of any Type for the defined string of text. 
    /// </summary>
    /// <param name="searchFor">The string to search for.</param>
    /// <param name="_objectsToSearch">The objects to search.</param>
    /// <param name="resultTexts">Out parameter. The string fields in the objects found, in order of their best match to the search string.</param>
    /// <param name="resultWeights">Out parameter. The search score for the keyword or phrase found in the returned objects.</param>
    /// <param name="caseSensitive">False by default. Set to true if the query is case-sensitive.</param>
    /// <param name="maxResults">The maximum number of returned objects. Default is 50.</param>
    /// <returns>The objects in which the searchFor string was found, or their closest matches.</returns>
    public object[] KeywordSearchInObjects( string searchFor, object[] _objectsToSearch, out string[][] resultTexts, out float[] resultWeights, bool caseSensitive = false, int maxResults = 50 ) {
        if (_objectsToSearch == null && objectsToSearch != null) {
            _objectsToSearch = objectsToSearch.ToArray();
        }
        //The goal is to return objects with string matches and weight how much each record matches
        if (!caseSensitive) {
            searchFor = searchFor.ToLower();
        }
        string[] keywords = searchFor.Split(' ');
        int keywordCount = keywords.Length;
        float keywordWeight = 1.0f / keywordCount;
        //Keep these results synced. Add to the weight when matches happen.
        List<object> _resultObjects = new List<object>();
        List<string[]> _resultTexts = new List<string[]>();
        List<float> _resultWeights = new List<float>();

        for (int i = 0; i < _objectsToSearch.Length; i++) {
            string[] stringsInObject = GetStringsFromObject(_objectsToSearch[i], true);
            string currFieldTitle = "";
            //If you get an exact match on SearchFor, that's good.
            for (int j = 0; j < stringsInObject.Length; j++) {
                if (stringsInObject[j].StartsWith("<") && stringsInObject[j].EndsWith(">")) {
                    currFieldTitle = stringsInObject[j];
                }
                string fieldContents = stringsInObject[j];
                if (!caseSensitive) {
                    fieldContents = fieldContents.ToLower();
                }
                if (fieldContents.Contains(searchFor)) {
                    //No direct string match of the search input.
                    if (!_resultObjects.Contains(_objectsToSearch[i])) {
                        _resultObjects.Add(_objectsToSearch[i]);
                        List<string> _result = new List<string>();
                        _result.Add(currFieldTitle);
                        _result.Add(stringsInObject[j]);
                        _resultTexts.Add(_result.ToArray());
                        _resultWeights.Add(1f);
                    } else if (_resultObjects.Contains(_objectsToSearch[i])) {
                        int resultIndex = _resultObjects.IndexOf(_objectsToSearch[i]);
                        List<string> _aggregateResult = new List<string>();
                        _aggregateResult.AddRange(_resultTexts[resultIndex]);
                        _aggregateResult.Add(currFieldTitle);
                        _aggregateResult.Add(stringsInObject[j]);
                        _resultTexts[resultIndex] = _aggregateResult.ToArray();
                        _resultWeights[resultIndex] += 1f;
                    }
                }
                //Try by keywords as well and see what you find.
                if (keywords.Length > 1) {
                    string[] fieldContentsSplit = fieldContents.Split(' ');
                    for (int k = 0; k < fieldContentsSplit.Length; k++) {
                        for (int l = 0; l < keywords.Length; l++) {
                            if (fieldContentsSplit[k].Contains(keywords[l])) {
                                //Found one of the Keywords
                                if (!_resultObjects.Contains(_objectsToSearch[i])) {
                                    _resultObjects.Add(_objectsToSearch[i]);
                                    List<string> _result = new List<string>();
                                    _result.Add(currFieldTitle);
                                    _result.Add(stringsInObject[j]);
                                    _resultTexts.Add(_result.ToArray());
                                    _resultWeights.Add(keywordWeight);
                                } else if (_resultObjects.Contains(_objectsToSearch[i])) {
                                    int resultIndex = _resultObjects.IndexOf(_objectsToSearch[i]);
                                    if (!_resultTexts[resultIndex].Contains(currFieldTitle)) {
                                        List<string> _aggregateResult = new List<string>();
                                        _aggregateResult.AddRange(_resultTexts[resultIndex]);
                                        _aggregateResult.Add(currFieldTitle);
                                        _aggregateResult.Add(stringsInObject[j]);
                                        _resultTexts[resultIndex] = _aggregateResult.ToArray();
                                    }
                                    _resultWeights[resultIndex] += keywordWeight;
                                }
                            }
                        }
                    }
                }
            }
        }
        resultWeights = _resultWeights.ToArray();
        resultTexts = _resultTexts.ToArray();
        object[] resultObjects = _resultObjects.ToArray();
        //Have to sort both ResultObjects AND ResultTexts BY ResultWeights to keep them aligned.
        Array.Sort(resultWeights, resultObjects);
        Array.Sort(_resultWeights.ToArray(), resultTexts);
        resultTexts = resultTexts.Reverse().ToArray();
        resultWeights = resultWeights.Reverse().ToArray();
        resultObjects = resultObjects.Reverse().ToArray();
        //Now we can clip it to the max entries
        if (resultObjects.Length > maxResults) {
            resultWeights = resultWeights.Take(maxResults).ToArray();
            resultTexts = resultTexts.Take(maxResults).ToArray();
            resultObjects = resultObjects.Take(maxResults).ToArray();
        }
        return resultObjects;
    }

    /// <summary>
    /// Gets strings from an object. Returns them in an array.
    /// </summary>
    /// <param name="dataObject">An object to search for strings, or collections of strings, or subclasses with strings / collections of strings.</param>
    /// <param name="returnArrayContents">Returns the string contents of arrays found in the object or its subclasses if true.</param>
    /// <param name="returnClassContents">Returns the string contents of fields in subclasses if true.</param>
    /// <returns>An array of all of the strings found in the object, with declarations for the field name in question.</returns>
    [Button]
    public string[] GetStringsFromObject( object dataObject, bool returnArrayContents = true, bool returnClassContents = true ) {
        Type type = dataObject.GetType();
        FieldInfo[] fields = type.GetFields();
        List<string> stringsFound = new List<string>();
        for (int i = 0; i < fields.Length; i++) {
            if (fields[i].FieldType == typeof(string)) {
                string stringValue = (string)fields[i].GetValue(dataObject);
                if (!string.IsNullOrEmpty(stringValue)) {
                    stringsFound.Add("<" + fields[i].Name + ">");
                    stringsFound.Add(stringValue);
                }
            }
            if (returnArrayContents) {
                if (fields[i].FieldType == typeof(string[])) {
                    string[] stringArrayContents = (string[])fields[i].GetValue(dataObject);
                    stringsFound.Add("<" + fields[i].Name + "[" + stringArrayContents.Length + "]>");
                    stringsFound.AddRange(stringArrayContents);
                } else if (fields[i].FieldType == typeof(List<string>)) {
                    List<string> stringArrayContents = (List<string>)fields[i].GetValue(dataObject);
                    stringsFound.Add("<" + fields[i].Name + "[" + stringArrayContents.Count + "]>");
                    stringsFound.AddRange(stringArrayContents);
                }
            }
            if (returnClassContents) {
                if (!fields[i].FieldType.IsClass) {
                    Type subclassType = fields[i].FieldType;
                    FieldInfo[] subclassFields = subclassType.GetFields();
                    for (int j = 0; j < subclassFields.Length; j++) {
                        if (subclassFields[j].FieldType == typeof(string)) {
                            string stringValue = (string)subclassFields[j].GetValue(dataObject);
                            if (!string.IsNullOrEmpty(stringValue)) {
                                stringsFound.Add("<" + fields[i].Name + "." + subclassFields[j].Name + ">");
                                stringsFound.Add(stringValue);
                            }
                        }
                        if (returnArrayContents) {
                            if (subclassFields[j].FieldType == typeof(string[])) {
                                string[] stringArrayContents = (string[])subclassFields[j].GetValue(dataObject);
                                stringsFound.Add("<" + fields[i].Name + "." + subclassFields[j].Name + "[" + stringArrayContents.Length + "]>");
                                stringsFound.AddRange(stringArrayContents);
                            } else if (subclassFields[j].FieldType == typeof(List<string>)) {
                                List<string> stringArrayContents = (List<string>)subclassFields[j].GetValue(dataObject);
                                stringsFound.Add("<" + fields[i].Name + "." + subclassFields[j].Name + "[" + stringArrayContents.Count + "]>");
                                stringsFound.AddRange(stringArrayContents);
                            }
                        }
                    }
                }
            }
        }
        return stringsFound.ToArray();
    }
}