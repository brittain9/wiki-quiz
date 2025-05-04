import Typography from '@mui/material/Typography';
import { motion } from 'framer-motion';
import * as React from 'react';
import { useTranslation } from 'react-i18next';

const TYPING_SPEED = 500;
const BACKSPACE_SPEED = 50;
const PAUSE_AFTER_WORD = 10000;
const PAUSE_BEFORE_NEXT_WORD = 500;

const AnimatedTopics: React.FC = () => {
  const { t } = useTranslation();
  const topics = React.useMemo(() => {
    const translatedTopics = t('hero.topics', { returnObjects: true });
    return Array.isArray(translatedTopics) ? translatedTopics : [];
  }, [t]);

  const [currentTopicIndex, setCurrentTopicIndex] = React.useState(0);
  const [displayText, setDisplayText] = React.useState('');
  const [isDeleting, setIsDeleting] = React.useState(false);
  const wordRef = React.useRef<HTMLSpanElement>(null);

  React.useEffect(() => {
    if (topics.length === 0) return;

    let timer: number;

    const tick = () => {
      const currentWord = topics[currentTopicIndex];

      if (!isDeleting && displayText === currentWord) {
        // Finished typing, pause before deleting
        timer = window.setTimeout(() => {
          setIsDeleting(true);
          tick();
        }, PAUSE_AFTER_WORD);
      } else if (isDeleting && displayText === '') {
        // Finished deleting, move to next word
        setIsDeleting(false);
        setCurrentTopicIndex((prevIndex) => (prevIndex + 1) % topics.length);
        timer = window.setTimeout(tick, PAUSE_BEFORE_NEXT_WORD);
      } else {
        // Typing or deleting
        setDisplayText((prevText) => {
          if (isDeleting) {
            return prevText.slice(0, -1);
          } else {
            return currentWord.slice(0, prevText.length + 1);
          }
        });
        timer = window.setTimeout(
          tick,
          isDeleting ? BACKSPACE_SPEED : TYPING_SPEED,
        );
      }
    };

    timer = window.setTimeout(tick, TYPING_SPEED);

    return () => window.clearTimeout(timer);
  }, [topics, currentTopicIndex, displayText, isDeleting]);

  if (topics.length === 0) {
    return null;
  }

  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'baseline',
        justifyContent: 'flex-start',
        textAlign: 'left',
        minWidth: '180px',
        maxWidth: '100vw',
        width: 'clamp(200px, 32vw, 420px)',
        position: 'relative',
        overflow: 'hidden',
        whiteSpace: 'nowrap',
        textOverflow: 'ellipsis',
        verticalAlign: 'bottom',
      }}
    >
      <Typography
        component="span"
        variant="h1"
        sx={{
          fontSize: 'inherit',
          color: 'var(--main-color)',
          transition: 'color 0.5s ease',
          display: 'inline',
          whiteSpace: 'nowrap',
          overflow: 'hidden',
          textOverflow: 'ellipsis',
          verticalAlign: 'bottom',
          position: 'relative',
          zIndex: 1,
          letterSpacing: 0,
        }}
      >
        <span
          ref={wordRef}
          style={{ padding: 0, margin: 0, letterSpacing: 0, display: 'inline' }}
        >
          {displayText}
        </span>
        <motion.span
          style={{
            position: 'relative',
            zIndex: 2,
            padding: 0,
            margin: 0,
            left: 0,
            display: 'inline',
            font: 'inherit',
            letterSpacing: 0,
            marginLeft: '-0.08em',
          }}
          initial={{ opacity: 1 }}
          animate={{ opacity: [1, 0, 1] }}
          transition={{ duration: 1, repeat: Infinity, ease: 'easeInOut' }}
        >
          |
        </motion.span>
      </Typography>
    </span>
  );
};

export default AnimatedTopics;
