package raven.abstractions.closure;

public class Delegates {
  public static class Delegate1<X> implements Action1<X> {
    @Override
    public void apply(X first) {
      // do nothing
    }
  }

  public static class Delegate2<X, Y> implements Action2<X, Y> {
    @Override
    public void apply(X first, Y second) {
      // do nothing
    }
  }

  public static class Delegate3<X, Y, Z> implements Action3<X, Y, Z> {
    @Override
    public void apply(X first, Y second, Z third) {
      // do nothing
    }
  }

  public static <X> Action1<X> delegate1() {
    return new Delegate1<>();
  }

  public static <X, Y> Action2<X, Y> delegate2() {
    return new Delegate2<>();
  }

  public static <X, Y, Z> Action3<X, Y, Z> delegate3() {
    return new Delegate3<>();
  }

  public static <X> Action1<X> combine(final Action1<X> first, final Action1<X> second) {
    if (first == null) {
      return second;
    }
    if (second == null) {
      return first;
    }
    return new Action1<X>() {

      @Override
      public void apply(X input) {
        if (first != null) {
          first.apply(input);
        }
        if (second != null) {
          second.apply(input);
        }
      }
    };
  }

}
