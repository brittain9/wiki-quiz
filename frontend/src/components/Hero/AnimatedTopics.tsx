import * as React from 'react';
import { useTranslation } from 'react-i18next';
import Typography from '@mui/material/Typography';
import './AnimatedTopics.css';

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
        timer = window.setTimeout(tick, isDeleting ? BACKSPACE_SPEED : TYPING_SPEED);
      }
    };

    timer = window.setTimeout(tick, TYPING_SPEED);

    return () => window.clearTimeout(timer);
  }, [topics, currentTopicIndex, displayText, isDeleting]);

  if (topics.length === 0) {
    return null;
  }

  return (
    <Typography
      component="span"
      variant="h1"
      sx={(theme) => ({
        fontSize: 'inherit',
        color: 'primary.main',
        transition: 'color 0.5s ease',
        ...theme.applyStyles('dark', {
          color: 'primary.light',
        }),
      })}
    >
      {displayText}
      <span className="cursor">|</span>
    </Typography>
  );
};

export default AnimatedTopics;
