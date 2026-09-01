import pytest
import pandas as pd
from src.data_processor import (
    process_top_coins,
    add_technical_indicators,
    format_currency,
    generate_fallback_history
)

def test_process_top_coins_empty():
    """Verify fallback behavior when empty data is passed."""
    df = process_top_coins([])
    assert isinstance(df, pd.DataFrame)
    assert not df.empty
    assert "name" in df.columns
    assert "current_price" in df.columns

def test_process_top_coins_cleaning():
    """Verify symbol capitalization and percentage fillna."""
    sample_data = [
        {"id": "bitcoin", "name": "Bitcoin", "symbol": "btc", "current_price": 50000.0, "price_change_percentage_24h": None}
    ]
    df = process_top_coins(sample_data)
    assert df["symbol"].iloc[0] == "BTC"
    assert df["price_change_percentage_24h"].iloc[0] == 0.0

def test_add_technical_indicators():
    """Verify SMA calculation and daily return columns creation."""
    dates = pd.date_range("2024-01-01", periods=10)
    prices = [100.0, 105.0, 102.0, 108.0, 110.0, 107.0, 112.0, 115.0, 120.0, 118.0]
    df = pd.DataFrame({"timestamp": dates, "price": prices})
    
    df_result = add_technical_indicators(df)
    
    assert "SMA_7" in df_result.columns
    assert "SMA_30" in df_result.columns
    assert "daily_return" in df_result.columns
    assert "volatility_7d" in df_result.columns
    assert df_result["SMA_7"].iloc[6] == pytest.approx(sum(prices[:7]) / 7, rel=1e-2)

def test_format_currency():
    """Verify financial string formatting for billions, millions, and standard values."""
    assert format_currency(1_500_000_000, "$") == "$1.50B"
    assert format_currency(2_350_000, "$") == "$2.35M"
    assert format_currency(1234.56, "$") == "$1,234.56"

def test_generate_fallback_history():
    """Verify synthetic history generator generates correct shape and columns."""
    df = generate_fallback_history("bitcoin", days=30)
    assert isinstance(df, pd.DataFrame)
    assert len(df) == 31
    assert "price" in df.columns
    assert "SMA_7" in df.columns
