let report;

/**
 * Embeds the Power BI Report in the 'reportContainer' div.
 * 
 * @param {string} embedUrl - The embed URL returned from the server
 * @param {string} embedToken - The Power BI embed token returned from the server
 */
function embedPowerBIReport(embedUrl, embedToken) {
    try {
        if (!window.powerbi) {
            console.error("Power BI client library (powerbi-client) not loaded.");
            return;
        }

        const models = window["powerbi-client"].models;

        // Build the configuration object using the token & URL from server
        const config = {
            type: "report",
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
        if (!embedContainer) {
            console.error("Could not find reportContainer element.");
            return;
        }

        // Embed the report into the container
        report = powerbi.embed(embedContainer, config);
        
        // Optionally handle events
        report.on("loaded", () => {
            console.log("Power BI Report loaded successfully.");
        });

        report.on("error", event => {
            console.error("Power BI Report error:", event.detail);
        });
    } catch (error) {
        console.error("Error embedding Power BI report:", error);
    }
}
