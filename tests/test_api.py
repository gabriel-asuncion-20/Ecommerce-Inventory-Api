import pytest
from src.api import fetch_top_coins, fetch_coin_history

def test_fetch_top_coins_returns_tuple():
    """Verify function returns tuple of (data, fallback_flag)."""
    data, is_fallback = fetch_top_coins(vs_currency="usd", top_n=5)
    assert isinstance(is_fallback, bool)
    if not is_fallback:
        assert isinstance(data, list)
        assert len(data) <= 5

def test_fetch_coin_history_returns_tuple():
    """Verify history fetch structure."""
    data, is_fallback = fetch_coin_history(coin_id="bitcoin", days=7)
    assert isinstance(is_fallback, bool)
    if not is_fallback:
        assert isinstance(data, dict)
        assert "prices" in data
