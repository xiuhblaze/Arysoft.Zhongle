using System;
using System.Linq.Expressions;
using System.Web.Mvc;

public static class MvcHtmlHelpers
{
    /// <summary>
    /// Presenta en la vista, la descripción indicada en los modelos, para el campo especificado.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="self"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://stackoverflow.com/questions/6578495/how-do-i-display-the-displayattribute-description-attribute-value/35343858#35343858
    /// </remarks>
    public static MvcHtmlString DescriptionFor<TModel, TValue>(this HtmlHelper<TModel> self, Expression<Func<TModel, TValue>> expression)
    {
        var metadata = ModelMetadata.FromLambdaExpression(expression, self.ViewData);
        var description = metadata.Description;

        return string.IsNullOrWhiteSpace(description) ? MvcHtmlString.Empty : MvcHtmlString.Create(string.Format(@"{0}", description));
    }

} // MvcHtmlHelpers
