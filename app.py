import streamlit as st
import pandas as pd
from datetime import datetime

from src.api import fetch_top_coins, fetch_coin_history
from src.data_processor import (
    process_top_coins,
    process_history,
    FALLBACK_COINS
)
from src.components.sidebar import render_sidebar
from src.components.metrics import render_kpi_cards
from src.components.charts import (
    render_price_chart,
    render_volume_chart,
    render_market_cap_distribution,
    render_top_coins_comparison
)

# 1. Page Configuration
st.set_page_config(
    page_title="CryptoIntelligence | Dashboard de Análisis de Mercado",
    page_icon="⚡",
    layout="wide",
    initial_sidebar_state="expanded"
)

# 2. Custom CSS Injection (Glassmorphism & Aesthetics)
st.markdown("""
    <style>
    .main-header {
        font-size: 2.2rem;
        font-weight: 800;
        background: linear-gradient(90deg, #8B5CF6 0%, #06B6D4 100%);
        -webkit-background-clip: text;
        -webkit-text-fill-color: transparent;
        margin-bottom: 0.2rem;
    }
    .sub-header {
        color: #94A3B8;
        font-size: 1.0rem;
        margin-bottom: 1.5rem;
    }
    .status-badge {
        background-color: #1E293B;
        border: 1px solid #334155;
        color: #10B981;
        padding: 0.25rem 0.75rem;
        border-radius: 9999px;
        font-size: 0.85rem;
        font-weight: 600;
        display: inline-block;
        margin-bottom: 1rem;
    }
    .stMetric {
        background: rgba(30, 41, 59, 0.7);
        border: 1px solid #334155;
        padding: 1rem;
        border-radius: 12px;
        backdrop-filter: blur(10px);
    }
    footer {
        visibility: hidden;
    }
    </style>
""", unsafe_allow_html=True)


def main():
    # Header Section
    st.markdown('<div class="main-header">⚡ CryptoIntelligence Dashboard</div>', unsafe_allow_html=True)
    st.markdown('<div class="sub-header">Plataforma de Inteligencia Financiera y Análisis Técnico de Mercado en Tiempo Real</div>', unsafe_allow_html=True)
    
    # 3. Data Fetching (Top Coins)
    raw_top_coins, is_fallback = fetch_top_coins(vs_currency="usd", top_n=10)
    top_coins_df = process_top_coins(raw_top_coins)
    
    # Status indicator badge
    if is_fallback:
        st.markdown('<div class="status-badge" style="color:#F59E0B; border-color:#F59E0B;">⚠️ Modo Respaldo Activo (Dataset Simulado / Offline)</div>', unsafe_allow_html=True)
    else:
        st.markdown(f'<div class="status-badge">🟢 Conectado a CoinGecko REST API | Última actualización: {datetime.now().strftime("%H:%M:%S")}</div>', unsafe_allow_html=True)

    # 4. Render Sidebar Controls
    available_coins_list = top_coins_df.to_dict(orient="records") if not top_coins_df.empty else FALLBACK_COINS
    settings = render_sidebar(available_coins_list)
    
    # Find selected coin details
    selected_coin_info = top_coins_df[top_coins_df["id"] == settings["coin_id"]]
    coin_data_dict = selected_coin_info.iloc[0].to_dict() if not selected_coin_info.empty else {}
    
    # Fetch Coin History
    raw_history, history_is_fallback = fetch_coin_history(
        coin_id=settings["coin_id"],
        vs_currency=settings["currency"],
        days=settings["days"]
    )
    history_df = process_history(raw_history)
    
    # 5. Main Content Tabs Layout
    tab1, tab2, tab3 = st.tabs(["📊 Análisis por Activo", "🌐 Visión General del Mercado", "💾 Exportar y Datos Brutos"])
    
    # Tab 1: Single Coin Technical Analysis
    with tab1:
        st.subheader(f"🔍 Análisis Detallado: {settings['coin_name']}")
        
        # Key Financial KPIs
        render_kpi_cards(coin_data_dict, history_df, settings["currency"])
        st.markdown("<br>", unsafe_allow_html=True)
        
        # Interactive Price & Indicators Chart
        render_price_chart(history_df, settings["coin_name"], settings)
        
        # Trading Volume
        if settings.get("show_volume"):
            render_volume_chart(history_df, settings["coin_name"])
            
    # Tab 2: Global Market Overview
    with tab2:
        st.subheader("🌐 Métricas Globales del Mercado Cripto")
        col_m1, col_m2 = st.columns(2)
        
        with col_m1:
            render_market_cap_distribution(top_coins_df)
            
        with col_m2:
            render_top_coins_comparison(top_coins_df)
            
        st.markdown("### 🏆 Ranking de Criptomonedas por Capitalización")
        st.dataframe(
            top_coins_df.style.format({
                "current_price": "${:,.2f}",
                "market_cap": "${:,.0f}",
                "price_change_percentage_24h": "{:+.2f}%",
                "total_volume": "${:,.0f}"
            }),
            use_container_width=True
        )

    # Tab 3: Data Inspection & CSV Export
    with tab3:
        st.subheader("💾 Inspección de Datos Limpios y Exportación")
        st.write("A continuación se muestra el resumen estructurado generado por el pipeline de limpieza en Pandas:")
        
        st.dataframe(history_df, use_container_width=True)
        
        csv_data = history_df.to_csv(index=False).encode('utf-8')
        st.download_button(
            label="📥 Descargar Dataset Histórico (CSV)",
            data=csv_data,
            file_name=f"{settings['coin_id']}_history_{settings['days']}d.csv",
            mime="text/csv"
        )

    # Project Footer
    st.markdown("---")
    st.markdown("""
        <div style="text-align: center; color: #64748B; font-size: 0.9rem;">
            <b>CryptoIntelligence Dashboard</b> | Proyecto de Portafolio para CV<br>
            <i>Tecnologías utilizadas: Python 3, Streamlit, Pandas, Plotly, Requests (REST API), Pytest</i>
        </div>
    """, unsafe_allow_html=True)


if __name__ == "__main__":
    main()
