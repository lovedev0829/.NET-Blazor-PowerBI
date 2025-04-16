window.embedPowerBIReport = function (embedUrl, embedToken) {
    const models = window['powerbi-client'].models;
    const config = {
        type: 'report',
        tokenType: models.TokenType.Embed,
        accessToken: embedToken,
        embedUrl: embedUrl,
        viewMode: models.ViewMode.View,
        settings: {
            panes: {
                filters: { visible: false },
                pageNavigation: { visible: false }
            },
            bars: {
                statusBar: { visible: true }
            }
        }
    };

    const embedContainer = document.getElementById("reportContainer");
    const report = powerbi.embed(embedContainer, config);

    report.on("loaded", () => {
        console.log("Power BI Report Loaded Successfully");
    });

    report.on("error", event => {
        console.error("Power BI Report Error:", event.detail);
    });
}; 