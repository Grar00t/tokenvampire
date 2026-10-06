namespace TokenVampire.I18n;

public static class BuiltinLocales
{
    private static readonly string[] Keys =
    [
        "report.title", "report.funded", "report.usage", "report.not_measured", "report.gaps",
        "report.boundary", "report.none", "report.items", "report.subs", "report.seats",
        "report.refunds", "report.credits"
    ];

    private static readonly Dictionary<string, string[]> Data = new(StringComparer.Ordinal)
    {
        ["fr"] =
        [
            "RAPPORT DE DOSSIER", "Financé (recharges de crédit, pas des dépenses)", "Consommation facturée",
            "NON MESURÉ", "Lacunes déclarées",
            "Un chiffre manquant est inconnu, pas zéro. Ceci n'est pas un droit juridique au remboursement.",
            "aucun", "élément(s)", "Abonnements", "Licences par siège", "Remboursements", "Crédits accordés"
        ],
        ["es"] =
        [
            "INFORME DEL CASO", "Financiado (recargas de crédito, no gasto)", "Consumo facturado",
            "NO MEDIDO", "Brechas declaradas",
            "Una cifra ausente es desconocida, no cero. Esto no constituye un derecho legal a reembolso.",
            "ninguno", "elemento(s)", "Suscripciones", "Licencias por puesto", "Reembolsos", "Créditos concedidos"
        ],
        ["ur"] =
        [
            "کیس رپورٹ", "فنڈنگ (کریڈٹ ری چارج، خرچ نہیں)", "بل شدہ استعمال",
            "ناپا نہیں گیا", "اعلان کردہ خلا",
            "غائب رقم نامعلوم ہے، صفر نہیں۔ یہ واپسی کا قانونی حق نہیں ہے۔",
            "کوئی نہیں", "آئٹم", "سبسکرپشنز", "سیٹ لائسنس", "رقم کی واپسی", "دیے گئے کریڈٹس"
        ],
        ["hi"] =
        [
            "मामले की रिपोर्ट", "फंडिंग (क्रेडिट टॉप-अप, खर्च नहीं)", "बिल किया गया उपयोग",
            "मापा नहीं गया", "घोषित अंतराल",
            "गायब आंकड़ा अज्ञात है, शून्य नहीं। यह रिफंड का कानूनी अधिकार नहीं है।",
            "कोई नहीं", "मद", "सदस्यताएँ", "सीट लाइसेंस", "रिफंड", "दिए गए क्रेडिट"
        ],
        ["zh"] =
        [
            "案件报告", "已充值（信用额度充值，非支出）", "已计费用量（消耗）",
            "未测量", "已声明的缺口",
            "缺失的数字表示未知，而非零。这不构成法律上的退款权利。",
            "无", "项", "订阅", "席位许可", "退款", "已授予的额度"
        ],
        ["ja"] =
        [
            "ケースレポート", "入金済み（クレジットのチャージであり、支出ではない）", "請求済み利用量（消費）",
            "未測定", "申告済みの欠落",
            "欠けている数値は不明であり、ゼロではありません。これは法的な返金請求権を意味しません。",
            "なし", "件", "サブスクリプション", "シート・ライセンス", "返金", "付与されたクレジット"
        ]
    };

    public static void AddAll(Catalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        foreach (var (tag, values) in Data)
        {
            if (values.Length != Keys.Length)
            {
                throw new InvalidOperationException("Locale " + tag + " has wrong value count");
            }
            for (var i = 0; i < Keys.Length; i++)
            {
                catalog.Set(tag, Keys[i], values[i]);
            }
        }
    }
}
